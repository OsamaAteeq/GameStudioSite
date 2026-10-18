namespace GameStudioSite.Models
{
    public class Service
    {
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string ShortDescription { get; set; } = "";
        public string Description { get; set; } = "";
        public string PicturePrimary { get; set; } = "";
        public string PictureSecondary { get; set; } = "";
    }

    public static class ServiceSeedData
    {
        public static List<Service> All => new()
        {
            new Service { Slug="development", Title = "Game Development", Description = "We bring your game to life with cutting-edge technology and expertise, crafting engaging experiences for PC, console, and mobile platforms. Our developers specialize in implementing game functions like physics, AI, and animation.", ShortDescription="Crafting immersive worlds and engaging gameplay experiences, tailored to your unique vision", PicturePrimary="/images/code-820275_12801.jpg", PictureSecondary="/images/pexels-kevin-ku-577585-scaled.jpg" },
            new Service { Slug="design",Title = "Game Design", Description = "Our team creates immersive experiences that captivate players, developing game elements and mechanics that bring your idea or story to life. We collaborate closely with clients to understand their vision and goals.", ShortDescription="Transforming imaginative concepts into captivating game mechanics, ensuring player satisfaction", PicturePrimary="/images/Design.jpg", PictureSecondary="/images/pexels-zia-moshtaghi-18787907-scaled.jpg" },
            new Service { Slug="multiplayer",Title = "Multiplayer Development", Description = "We develop local and online multiplayer games that connect players worldwide and create an exciting shared experience, using Multiplay, PlayFab, Photon, and other top-notch services.", ShortDescription="Building seamless multiplayer experiences, connecting players worldwide with smooth gameplay", PicturePrimary="/images/Multiplayer.jpg", PictureSecondary="/images/pexels-cottonbro-studio-40096061.jpg" },
            new Service { Slug="art",Title = "2D/3D Art", Description = "Our artists create stunning visuals that bring your game to life, including 2D and 3D assets, characters, environments, and special effects, across various art styles and software.", ShortDescription="Bringing your game to life with stunning visuals, from concept art to high quality 2D/3D models", PicturePrimary="/images/Art.jpg", PictureSecondary="/images/deer-3275594_1280.jpg" },
            new Service { Slug="gamification",Title = "Gamification", Description = "We introduce engaging game mechanics into non-gaming applications to boost user engagement, retention, and motivation through strategically designed reward systems.", ShortDescription=" Enhancing application's user engagement and retention through strategic game elements", PicturePrimary="/images/Gamefication.jpg", PictureSecondary="/images/pexels-tima-miroshnichenko-6614832.jpg" },
            new Service { Slug="consultation",Title = "Consultation", Description = "Providing expert guidance and insights to elevate your project, tailored to your specific needs and goals.", ShortDescription="Providing expert guidance and insights to elevate your project, tailored to your specific needs and goals", PicturePrimary="/images/Consultation.jpg", PictureSecondary="/images/consulting-3031678_1280.jpg" }
        };
    }
}