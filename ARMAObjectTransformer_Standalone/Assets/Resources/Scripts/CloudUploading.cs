using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Net;


public class CloudUploading : MonoBehaviour 
{
	private string access_key = ""; // Server access key
	private string secret_key = ""; // Server secret key
	private static string url = @"https://vws.vuforia.com";

	private byte[] requestBytesArray;

	public void CallPostTarget(Texture2D tex, string targetName, string metadataStr)
	{
		StartCoroutine (PostNewTarget(tex, targetName, metadataStr));
	}

	IEnumerator PostNewTarget(Texture2D tex, string targetName, string metadataStr)
	{
		string requestPath = "/targets";
		string serviceURI = url + requestPath;
		string httpAction = "POST";
		string contentType = "application/json";
		string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime()); 

		Debug.Log(date);

		// if your texture2d has RGb24 type, don't need to redraw new texture2d
		byte[] image = tex.EncodeToPNG();

		byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
		PostNewTrackableRequest model = new PostNewTrackableRequest();
		model.name = targetName;
		model.width = 64.0f; // don't need same as width of texture
		model.image = System.Convert.ToBase64String(image);

		model.application_metadata = System.Convert.ToBase64String(metadata);
		string requestBody = JsonUtility.ToJson(model);

		HttpWebRequest httpWReq = (HttpWebRequest) HttpWebRequest.Create(serviceURI);

		MD5 md5 = MD5.Create();
		var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < contentMD5bytes.Length; i++)
		{
			sb.Append(contentMD5bytes[i].ToString("x2"));
		}

		string contentMD5 = sb.ToString();

		string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, contentType, date, requestPath);

		HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
		byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
		MemoryStream stream = new MemoryStream(sha1Bytes);
		byte[] sha1Hash = sha1.ComputeHash(stream);
		string signature = System.Convert.ToBase64String(sha1Hash);

		Debug.Log("<color=green>Signature: "+signature+"</color>");

		// Make the Web Request
		Dictionary<string, string> headers = new Dictionary<string, string> ();
		headers.Add ("host", url);
		headers.Add ("date", date);
		headers.Add ("content-type", contentType);
		headers.Add ("authorization", string.Format("VWS {0}:{1}", access_key, signature));
		WWW request = new WWW(serviceURI, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(model)), headers);
		yield return request;

		if (request.error != null)
		{
			Debug.Log("request error: " + request.error);

			// Try doing a PUT request instead

		}
		else
		{
			Debug.Log("request success");
			Debug.Log("returned data" + request.text);
		}

	}
}


public class PostNewTrackableRequest
{
	public string name;
	public float width;
	public string image;
	public string application_metadata;
}