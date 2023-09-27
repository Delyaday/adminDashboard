namespace Foundation.Utils
{
    public class AuthenticationSettings
    {
        public string Secret { get; set; }

        /// <summary>
        /// Время жизни токена в днях. 
        /// </summary>
        public int RefreshTokenTTL { get; set; }
    }
}
