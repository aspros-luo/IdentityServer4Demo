namespace Id4AuthorityCenter.Config
{
    public class AppSettings
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string AppIdWithAgentApp { get; set; }
        public string AppSecretWithAgentApp { get; set; }
        public string AppIdWithApp { get; set; }
        public string AppSecretWithApp { get; set; }

        public string IssuerUri { get; set; }
		public string AppIdWithMiniProgram { get; set; }
		public string AppSecretWithMiniProgram { get; set; }


        #region Base Setting
        /// <summary>
        /// MongodbStr
        /// </summary>
        public string MongodbStr { get; set; }
        /// <summary>
        /// token 有效时间
        /// </summary>
        public int AccessTokenLifetime { get; set; }
        /// <summary>
        /// refresh token 有效时间
        /// </summary>
        public int SlidingRefreshTokenLifetime { get; set; }
        /// <summary>
        /// 证书名称
        /// </summary>
        public string CertsName { get; set; }
        /// <summary>
        /// 证书密码
        /// </summary>
        public string CertsPwd { get; set; }
        #endregion
    }
}
