using System;
using System.Collections.Generic;
using System.Globalization;
using BlockPuzzleGameToolkit.Scripts.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Quester
{
    public class PictureManager
    {
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