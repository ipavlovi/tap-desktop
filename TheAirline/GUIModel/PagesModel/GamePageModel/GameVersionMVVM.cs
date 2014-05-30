namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Linq;
    using System.Reflection;

    public class GameVersionMVVM
    {
        public string CurrentVersion
        {
            get
            {
                CustomAttributeData assemblyInformationalVersion =
                    Assembly.GetExecutingAssembly()
                        .CustomAttributes.FirstOrDefault(
                            att => att.AttributeType == typeof(AssemblyInformationalVersionAttribute));

                if (assemblyInformationalVersion != null)
                {
                    return string.Format(
                        "Game Version: {0}",
                        assemblyInformationalVersion.ConstructorArguments[0].ToString().Trim('"'));
                }

                return "unknown version";
            }
        }
    }
}
