use std::{
    fs::File,
    io::{BufRead, Read, Write},
};

pub mod yggdrasil;
use anyhow::{Context, Result};

fn get_session() -> Result<yggdrasil::SavedSession> {
    if let Ok(mut file) = File::open("../../session.json") {
        let mut buf = String::new();
        file.read_to_string(&mut buf)
            .with_context(|| "couldn't read for session.json")?;
        drop(file);
        let mut session: yggdrasil::SavedSession = serde_json::from_str(&buf)?;
        if let Err(..) = yggdrasil::check() {
            eprintln!("[MCAH] No internet, creating offline session.");
            let username = std::env::args().nth(1).unwrap();
            let uuid = yggdrasil::make_offline_uuid(&username);

            let profile = yggdrasil::MCProfile {
                id: uuid,
                name: username.clone(),
                legacy: false,
            };

            return Ok(yggdrasil::SavedSession {
                is_offline: true,
                email: String::new(),
                auth: yggdrasil::AuthenticateResponse {
                    access_token: "00000000-0000-0000-0000-000000000000".to_string(),
                    client_token: "00000000-0000-0000-0000-000000000000".to_string(),
                    user: Some(yggdrasil::MCUser {
                        id: uuid.to_string(),
                        properties: vec![],
                    }),
                    available_profiles: vec![profile.clone()],
                    selected_profile: Some(profile),
                },
            });
        }
        let refr = yggdrasil::with_response(&yggdrasil::RefreshRequest {
            access_token: session.auth.access_token.clone(),
            client_token: session.auth.client_token.clone(),
            request_user: true,
        });
        if let Ok(resp) = refr {
            session.auth.access_token = resp.access_token;
            session.auth.selected_profile = Some(resp.selected_profile);
            session.auth.user = resp.user;
            let mut sessfile =
                File::create("../../session.json").with_context(|| "couldn't open session.json")?;
            sessfile
                .write_all(&serde_json::to_vec_pretty(&session)?)
                .with_context(|| "couldn't write to session.json")?;
            return Ok(session);
        } else {
            let e = refr.unwrap_err();
            println!("[MCAH] Couldn't open session.json! {}", e);
            std::fs::remove_file("../../session.json")?;
        }
    }
    let (email, resp) = loop {
        let email = {
            print!("[MCAH] Email: ");
            std::io::stdout().flush()?;
            std::io::stdin().lock().lines().next().unwrap()?
        };
        let password = rpassword::prompt_password_stderr("[MCAH] Password: ")?;

        let resp = yggdrasil::with_response(&yggdrasil::AuthenticateRequest {
            agent: Some(yggdrasil::AgentData {
                name: "Minecraft".into(),
                version: 1,
            }),
            username: email.clone(),
            password: password,
            client_token: None,
            request_user: true,
        });
        if let Ok(o) = resp {
            break (email, o);
        } else if let Err(e) = resp {
            println!("[MCAH] {}", e);
        }
    };
    let session = yggdrasil::SavedSession {
        auth: resp,
        email: email.clone(),
        is_offline: false,
    };
    let mut sessfile =
        File::create("../../session.json").with_context(|| "couldn't open sessions.json")?;
    sessfile
        .write_all(
            &serde_json::to_vec_pretty(&session).with_context(|| "couldn't serialize session")?,
        )
        .with_context(|| "couldn't write to sessions.json")?;
    Ok(session)
}

fn main() -> Result<()> {
    let session = get_session()?;
    let mut args = vec![];
    for arg in std::env::args() {
        args.push(
            arg.replace("@ACCESSTOKEN", &session.auth.access_token)
                .replace(
                    "@USERNAME",
                    &session.auth.selected_profile.as_ref().unwrap().name,
                )
                .replace(
                    "@UUID",
                    &session
                        .auth
                        .selected_profile
                        .as_ref()
                        .unwrap()
                        .id
                        .to_string(),
                ),
        );
    }
    println!("[MCAH] Starting Minecraft!");
    #[cfg(unix)]
    {
        Err(exec::Command::new(&args[2])
            .args(&args.into_iter().skip(3).collect::<Vec<_>>())
            .exec())
        .with_context(|| "couldn't start minecraft")?
    }
    #[cfg(not(unix))]
    {
        std::process::Command::new(&args[2])
            .args(args.into_iter().skip(3))
            .status()
            .with_context(|| "couldn't start minecraft")?;
        Ok(())
    }
}
