// This should really be a crate.

use attohttpc::{Response, StatusCode};
use serde::{de::DeserializeOwned, Deserialize, Serialize};
use std::fmt::Debug;
use thiserror::Error;
use uuid::{Uuid, Variant, Version};

#[derive(Debug, Error)]
pub enum YggdrasilError {
    #[error(transparent)]
    Api(#[from] MCLoginError),
    #[error(transparent)]
    Request(#[from] attohttpc::Error),
}

pub trait YggdrasilSendable {
    const PATH: &'static str;
}

pub trait YggdrasilSend: YggdrasilSendable + Serialize + Debug {
    type Response: DeserializeOwned;
}

pub trait YggdrasilOneoff: YggdrasilSendable + Serialize {}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct AuthenticateRequest {
    pub agent: Option<AgentData>,
    pub username: String,
    pub password: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub client_token: Option<String>,
    pub request_user: bool,
}

impl YggdrasilSendable for AuthenticateRequest {
    const PATH: &'static str = "authenticate";
}
impl YggdrasilSend for AuthenticateRequest {
    type Response = AuthenticateResponse;
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct AgentData {
    pub name: String,
    pub version: i32,
}

#[derive(Serialize, Deserialize, Debug, Clone, Error)]
#[serde(rename_all = "camelCase")]
#[error("{error}: {error_message}")]
pub struct MCLoginError {
    pub error: String,
    pub error_message: String,
    pub cause: Option<String>,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct MCProfile {
    pub id: Uuid,
    pub name: String,
    #[serde(default)]
    pub legacy: bool,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct AuthenticateResponse {
    pub access_token: String,
    pub client_token: String,
    #[serde(default)]
    pub available_profiles: Vec<MCProfile>,
    pub selected_profile: Option<MCProfile>,
    pub user: Option<MCUser>,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct MCUser {
    pub id: String,
    #[serde(default)]
    pub properties: Vec<MCUserProperty>,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct MCUserProperty {
    pub name: String,
    pub value: String,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct SavedSession {
    #[serde(flatten)]
    pub auth: AuthenticateResponse,
    pub email: String,
    pub is_offline: bool,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct RefreshRequest {
    pub access_token: String,
    pub client_token: String,
    #[serde(default)]
    pub request_user: bool,
}

impl YggdrasilSendable for RefreshRequest {
    const PATH: &'static str = "refresh";
}
impl YggdrasilSend for RefreshRequest {
    type Response = RefreshResponse;
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct RefreshResponse {
    pub access_token: String,
    pub client_token: String,
    pub selected_profile: MCProfile,
    pub user: Option<MCUser>,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct ValidateRequest {
    pub access_token: String,
    pub client_token: Option<String>,
}

impl YggdrasilSendable for ValidateRequest {
    const PATH: &'static str = "validate";
}
impl YggdrasilOneoff for ValidateRequest {}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct SignoutRequest {
    pub username: String,
    pub password: String,
}

impl YggdrasilSendable for SignoutRequest {
    const PATH: &'static str = "signout";
}
impl YggdrasilOneoff for SignoutRequest {}

#[derive(Serialize, Deserialize, Debug, Clone)]
#[serde(rename_all = "camelCase")]
pub struct InvalidateRequest {
    pub access_token: String,
    pub client_token: String,
}

impl YggdrasilSendable for InvalidateRequest {
    const PATH: &'static str = "invalidate";
}
impl YggdrasilOneoff for InvalidateRequest {}

fn throw_error(res: Response) -> Result<Response, YggdrasilError> {
    if res.status() != StatusCode::OK {
        Err(YggdrasilError::Api(res.json()?))
    } else {
        Ok(res)
    }
}

pub fn check() -> Result<(), attohttpc::Error> {
    attohttpc::head("https://authserver.mojang.com/").send()?;
    Ok(())
}

pub fn with_response<T: YggdrasilSend>(req: &T) -> Result<T::Response, YggdrasilError> {
    let url = "https://authserver.mojang.com/".to_owned() + T::PATH;
    let res = attohttpc::post(&url).json(req)?.send()?;
    let res = throw_error(res)?;
    Ok(res.json()?)
}

pub fn oneoff<T: YggdrasilOneoff>(req: &T) -> Result<(), YggdrasilError> {
    let url = "https://authserver.mojang.com/".to_owned() + T::PATH;
    let res = attohttpc::post(&url).json(req)?.send()?;
    throw_error(res)?;
    Ok(())
}

pub fn make_offline_uuid(username: &str) -> Uuid {
    let mut hash = sha1::Sha1::new();

    hash.update("OfflinePlayer:".as_bytes());
    hash.update(username.as_bytes());

    let buffer = hash.digest().bytes();

    let mut bytes = uuid::Bytes::default();
    bytes.copy_from_slice(&buffer[..16]);

    let mut builder = uuid::Builder::from_bytes(bytes);
    builder
        .set_variant(Variant::RFC4122)
        .set_version(Version::Sha1);

    builder.build()
}
