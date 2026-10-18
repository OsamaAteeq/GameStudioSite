using GameStudioSite.Models;

namespace GameStudioSite.Services
{
    public interface IEmailService
    {
        Task SendContactFormEmailAsync(ContactFormModel form);
    }
}