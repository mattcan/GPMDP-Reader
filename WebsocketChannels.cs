using System;
using System.ComponentModel;
using System.Reflection;

namespace gpmdp_rdr
{
    public enum Channel
    {
        [Description("API_VERSION")]
        API_VERSION,

        [Description("playState")]
        PLAY_STATE,

        [Description("track")]
        TRACK,

        [Description("lyrics")]
        LYRICS,

        [Description("time")]
        TIME,

        [Description("rating")]
        RATING,

        [Description("shuffle")]
        SHUFFLE,

        [Description("repeat")]
        REPEAT,

        [Description("playlists")]
        PLAYLISTS,

        [Description("queue")]
        QUEUE,

        [Description("search-results")]
        SEARCH_RESULTS
    }

    public static class ChannelExtensions
    {
        /// <summary>
        /// Pull the data out of the Description attribute
        /// Basically this: https://stackoverflow.com/a/1415187/298281 with
        /// a few changes to avoid null checks outside this method and to be
        /// less generic :o
        /// </summary>
        /// <param name="value">The Channel enum</param>
        /// <returns>Text from Description attribute or empty string</returns>
        public static string GetDescription(this Channel value) {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name == null) { return string.Empty; }

            FieldInfo field = type.GetField(name);
            if (field == null) { return string.Empty; }

            DescriptionAttribute attr = Attribute
                .GetCustomAttribute(
                    field,
                    typeof(DescriptionAttribute)
                ) as DescriptionAttribute;
            
            return attr != null ? attr.Description : string.Empty;
        }
    }
}