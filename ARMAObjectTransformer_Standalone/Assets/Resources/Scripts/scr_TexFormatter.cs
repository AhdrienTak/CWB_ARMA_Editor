using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_TexFormatter : MonoBehaviour {

	public static TextureFormat Convert(string textureFormat) {
		switch (textureFormat) {
		case "Alpha8":
			return TextureFormat.Alpha8;
		case "ARGB4444":
			return TextureFormat.ARGB4444;
		case "RGB24":
			return TextureFormat.RGB24;
		case "RGBA32":
			return TextureFormat.RGBA32;
		case "ARGB32":
			return TextureFormat.ARGB32;
		case "RGB565":
			return TextureFormat.RGB565;
		case "R16":
			return TextureFormat.R16;
		case "DXT1":
			return TextureFormat.DXT1;
		case "DXT5":
			return TextureFormat.DXT5;
		case "RGBA4444":
			return TextureFormat.RGBA4444;
		case "BGRA32":
			return TextureFormat.BGRA32;
		case "RHalf":
			return TextureFormat.RHalf;
		case "RGHalf":
			return TextureFormat.RGHalf;
		case "RGBAHalf":
			return TextureFormat.RGBAHalf;
		case "RFloat":
			return TextureFormat.RFloat;
		case "RGFloat":
			return TextureFormat.RGFloat;
		case "RGBAFloat":
			return TextureFormat.RGBAFloat;
		case "YUY2":
			return TextureFormat.YUY2;
		case "RGB9e5Float":
			return TextureFormat.RGB9e5Float;
		case "BC4":
			return TextureFormat.BC4;
		case "BC5":
			return TextureFormat.BC5;
		case "BC6H":
			return TextureFormat.BC6H;
		case "BC7":
			return TextureFormat.BC7;
		case "DXT1Crunched":
			return TextureFormat.DXT1Crunched;
		case "DXT5Crunched":
			return TextureFormat.DXT5Crunched;
		case "PVRTC RGB2":
		case "PVRTC_RGB2":
			return TextureFormat.PVRTC_RGB2;
		case "PVRTC RGBA2":
		case "PVRTC_RGBA2":
			return TextureFormat.PVRTC_RGBA2;
		case "PVRTC RGB4":
		case "PVRTC_RGB4":
			return TextureFormat.PVRTC_RGB4;
		case "PVRTC RGBA4":
		case "PVRTC_RGBA4":
			return TextureFormat.PVRTC_RGBA4;
		case "ETC RGB4":
		case "ETC_RGB4":
			return TextureFormat.ETC_RGB4;
		case "ATC RGB4":
		case "ATC_RGB4":
			return TextureFormat.ATC_RGB4;
		case "ATC RGBA8":
		case "ATC_RGBA8":
			return TextureFormat.ATC_RGBA8;
		case "EAC R":
		case "EAC_R":
			return TextureFormat.EAC_R;
		case "EAC R SIGNED":
		case "EAC_R_SIGNED":
			return TextureFormat.EAC_R_SIGNED;
		case "EAC RG SIGNED":
		case "EAC_RG_SIGNED":
			return TextureFormat.EAC_RG_SIGNED;
		case "ETC2 RGB":
		case "ETC2_RGB":
			return TextureFormat.ETC2_RGB;
		case "ETC2 RGBA1":
		case "ETC2_RGBA1":
			return TextureFormat.ETC2_RGBA1;
		case "ETC2 RGBA8":
		case "ETC2_RGBA8":
			return TextureFormat.ETC2_RGBA8;
		case "ASTC RGB 4x4":
		case "ASTC_RGB_4x4":
			return TextureFormat.ASTC_RGB_4x4;
		case "ASTC RGB 5x5":
		case "ASTC_RGB_5x5":
			return TextureFormat.ASTC_RGB_5x5;
		case "ASTC RGB 6x6":
		case "ASTC_RGB_6x6":
			return TextureFormat.ASTC_RGB_6x6;
		case "ASTC RGB 8x8":
		case "ASTC_RGB_8x8":
			return TextureFormat.ASTC_RGB_8x8;
		case "ASTC RGB 10x10":
		case "ASTC_RGB_10x10":
			return TextureFormat.ASTC_RGB_10x10;
		case "ASTC RGB 12x12":
		case "ASTC_RGB_12x12":
			return TextureFormat.ASTC_RGB_12x12;
		case "ASTC RGBA 4x4":
		case "ASTC_RGBA_4x4":
			return TextureFormat.ASTC_RGBA_4x4;
		case "ASTC RGBA 5x5":
		case "ASTC_RGBA_5x5":
			return TextureFormat.ASTC_RGBA_5x5;
		case "ASTC RGBA 6x6":
		case "ASTC_RGBA_6x6":
			return TextureFormat.ASTC_RGBA_6x6;
		case "ASTC RGBA 8x8":
		case "ASTC_RGBA_8x8":
			return TextureFormat.ASTC_RGBA_8x8;
		case "ASTC RGBA 10x10":
		case "ASTC_RGBA_10x10":
			return TextureFormat.ASTC_RGBA_10x10;
		case "ASTC RGBA 12x12":
		case "ASTC_RGBA_12x12":
			return TextureFormat.ASTC_RGBA_12x12;
		case "ETC RGB4 3DS":
		case "ETC_RGB4_3DS":
			return TextureFormat.ETC_RGB4_3DS;
		case "ETC RGBA8 3DS":
		case "ETC_RGBA8_3DS":
			return TextureFormat.ETC_RGBA8_3DS;
		case "RG16":
			return TextureFormat.RG16;
		case "R8":
			return TextureFormat.R8;
		case "ETC RGB4Crunched":
		case "ETC_RGB4Crunched":
			return TextureFormat.ETC_RGB4Crunched;
		case "ETC2 RGBA8Crunched":
		case "ETC2_RGBA8Crunched":
			return TextureFormat.ETC2_RGBA8Crunched;
		default:
			return TextureFormat.ARGB32;
		}
	}
}
