namespace GameStudioSite.Models
{
    public class ErrorPageModel
    {
        public int Code { get; init; }
        public string Heading { get; init; } = "";
        public string Message { get; init; } = "";

        public static ErrorPageModel For(int code) => code switch
        {
            400 => new()
            {
                Code = code,
                Heading = "That didn't look\u00A0right",
                Message = "Something in the address or the form data was malformed. Go back and try again."
            },
            401 => new()
            {
                Code = code,
                Heading = "Sign-in needed",
                Message = "This page is only for signed-in users."
            },
            403 => new()
            {
                Code = code,
                Heading = "Access denied",
                Message = "You don't have permission to open this page."
            },
            405 => new()
            {
                Code = code,
                Heading = "That\u00A0action isn't\u00A0allowed",
                Message = "We do not accept this kind of request. Try opening it from a link instead."
            },
            408 => new()
            {
                Code = code,
                Heading = "The\u00A0request timed\u00A0out",
                Message = "It took too long to reach us. Check your connection and try again."
            },
            413 => new()
            {
                Code = code,
                Heading = "That\u00A0file is too\u00A0large",
                Message = "The upload was bigger than we accept. Try a smaller file."
            },
            415 => new()
            {
                Code = code,
                Heading = "Unsupported file\u00A0type",
                Message = "We couldn't use that kind of file. Try a different format."
            },
            429 => new()
            {
                Code = code,
                Heading = "Slow\u00A0down a\u00A0bit",
                Message = "Those were too many requests in a short time. Wait a moment and try again."
            },
            >= 500 and < 600 => new()
            {
                Code = code,
                Heading = "Something went\u00A0wrong on\u00A0our\u00A0side",
                Message = "We are sorry. It's not you, it's us. Please try again in a moment."
            },
            _ => new()
            {
                Code = code,
                Heading = "Something went\u00A0wrong",
                Message = "We couldn't complete that request. Please try again in a moment."
            },
        };
    }
}