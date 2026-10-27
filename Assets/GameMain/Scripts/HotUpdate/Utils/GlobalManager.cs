using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts;

namespace Quester
{
    public class GlobalManager : MonoSingleton<GlobalManager>
    {
        private SeasonShape _seasonShape;

        public SeasonShape SeasonShape
        {
            get => _seasonShape;
            set => _seasonShape = value;
        }
        
        public bool IsHardLevel(int level)
        {
            return level % 5 == 0 || level == _seasonShape.MaxLevel;;
        }

        private static bool HaveSpecialPictures(string countryCode)
        {
            HashSet<string> countryCodeSet = new HashSet<string>()
            {
                "US"
            };
            return countryCodeSet.Contains(countryCode);
        }
        
        public static string GetLocalName(int pictureIndex)
        {
            var pictureName = $"{pictureIndex}.jpg";
            var countryCode = CountryHelper.GetCountryCode();
            if (!string.IsNullOrEmpty(countryCode) && HaveSpecialPictures(countryCode))
            {
                pictureName = $"{countryCode}/{pictureName}";
            }
            else
            {
                pictureName = $"US/{pictureName}";
            }
            return pictureName;
        }

        public static string GetPictureUrl(string pictureName)
        {
            return $"https://assets-1301567094.cos.ap-beijing.myqcloud.com/block-bang/pictures/{pictureName}";
        }
    }
}