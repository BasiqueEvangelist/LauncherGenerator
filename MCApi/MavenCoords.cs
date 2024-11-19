namespace MCApi
{
    public class MavenCoords
    {
        private string[] split;
        public MavenCoords(string coords)
        {
            split = coords.Split(":");
            if (split.Length == 3)
            {
                split = new string[] { split[0], split[1], "", split[2], "", "jar" };
            }
        }
        public string GroupID
        {
            get =>
                split[0];
            set =>
                split[0] = value;
        }
        public string ArtifactID
        {
            get =>
                split[1];
            set =>
                split[1] = value;
        }
        public string PlatformID
        {
            get =>
                split[2];
            set =>
                split[2] = value;
        }
        public string Version
        {
            get =>
                split[3];
            set =>
                split[3] = value;
        }
        public string Classifier
        {
            get =>
                split[4];
            set =>
                split[4] = value;
        }
        public string FileType
        {
            get =>
                split[5];
            set =>
                split[5] = value;
        }
        public string LibraryPath => Path.Combine(
            GroupID.Replace('.', Path.DirectorySeparatorChar),
            ArtifactID,
            Version,
            ArtifactID + (PlatformID == "" ? "" : "-" + PlatformID) + "-" + Version + (Classifier == "" ? "" : "-" + Classifier) + "." + FileType);
    }
}
