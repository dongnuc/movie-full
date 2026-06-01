using Movie_StructureCode.Contract.Emails;

namespace Movie_StructureCode.Application.Abstractions.Services.Email
{
    /// <summary>
    /// Email Service Interface - định nghĩa các phương thức gửi email
    /// </summary>
    public interface IEmailServices
    {
        /// <summary>
        /// Gửi email với thông điệp tùy chỉnh
        /// </summary>
        /// <param name="message">Đối tượng Message chứa thông tin email</param>
        /// <returns>Task hoàn thành khi email được gửi</returns>
        public Task SendEmail(Message message);

        /// <summary>
        /// Gửi email xác nhận tài khoản
        /// </summary>
        /// <param name="email">Địa chỉ email của người dùng</param>
        /// <param name="confirmationLink">Link xác nhận tài khoản</param>
        /// <returns>Task hoàn thành khi email được gửi</returns>
        public Task SendMailConfirmAsync(string email, string confirmationLink);

        /// <summary>
        /// Gửi email quên mật khẩu
        /// </summary>
        /// <param name="email">Địa chỉ email của người dùng</param>
        /// <param name="token">Token để đặt lại mật khẩu</param>
        /// <returns>Task hoàn thành khi email được gửi</returns>
        public Task SendMailForgotPassWord(string email, string token);
    }
}
