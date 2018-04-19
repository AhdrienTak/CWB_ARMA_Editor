using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Net;


public class CloudUploading : MonoBehaviour {
	[SerializeField]
	private readonly string access_key = "2485b9476b8eef4efc9ae4965bfe5c8e9c591e75"; // Server access key
	[SerializeField]
	private readonly string secret_key = "6577e891e011549b90cb41f07a0bb80c9e59210d"; // Server secret key
	[SerializeField]
	private readonly static string url = @"https://vws.vuforia.com";

	private byte[] requestBytesArray;

	private bool successfullyDeactivated;
	private bool successfullyDeleted;


	public IEnumerator CallPostTarget(Texture2D tex, string targetName, string metadataStr) {
		yield return StartCoroutine (PostNewTarget(tex, targetName, metadataStr));
	}

	public IEnumerator CallDeleteTarget(string markerID) {
		successfullyDeactivated = false;
		yield return StartCoroutine (DeactivateTarget(markerID));

		successfullyDeleted = false;
		while (!successfullyDeleted) {
			yield return new WaitForSeconds(3);
			yield return StartCoroutine (DeleteTarget (markerID));
		} 
	}

	IEnumerator PostNewTarget(Texture2D tex, string targetName, string metadataStr) {
		string requestPath = "/targets";
		string serviceURI = url + requestPath;
		string httpAction = "POST";
		string contentType = "application/json";
		string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime()); 

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
		for (int i = 0; i < contentMD5bytes.Length; i++) {
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

		if (request.error != null && request.error.ToString().Contains("403")) {
			// Attempt to replace the target if it already exists
			GameObject.Find("MASTER_Controller").GetComponent<scr_SceneController>().DebugMessage("Attempting to replace old marker...");
			yield return StartCoroutine (PutNewTarget(tex, targetName, metadataStr));

		}
		else if (request.error != null) {
			GameObject.Find("MASTER_Controller").GetComponent<scr_SceneController>().DebugMessage(request.error, "error");
		}
		else {
			string markerID = request.text.Substring (request.text.IndexOf("target_id\":\""));
			markerID = markerID.Substring (12);
			markerID = markerID.Substring (0, markerID.Length - 2);
			// Save the target ID
			GameObject.Find("MASTER_Controller").GetComponent<scr_SceneController>().DebugMessage("Marker ID Created: " + markerID);
			GameObject.Find("MASTER_Controller").GetComponent<scr_SceneController>().setMarkerID(markerID);
		}

	}

	IEnumerator PutNewTarget(Texture2D tex, string targetName, string metadataStr) {
		string requestPath = "/targets/" + GameObject.Find("MASTER_Controller").GetComponent<scr_SceneController>().getMarkerID();
		string serviceURI = url + requestPath;
		string httpAction = "PUT";
		string contentType = "application/json";
		string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime()); 

		// if your texture2d has RGb24 type, don't need to redraw new texture2d
		byte[] image = tex.EncodeToPNG();

		byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
		PutNewTrackableRequest model = new PutNewTrackableRequest();
		model.name = targetName;
		model.width = 64.0f; // don't need same as width of texture
		model.image = System.Convert.ToBase64String(image);
		model.active_flag = true;
		model.application_metadata = System.Convert.ToBase64String(metadata);
		string requestBody = JsonUtility.ToJson(model);

		HttpWebRequest httpWReq = (HttpWebRequest) HttpWebRequest.Create(serviceURI);

		MD5 md5 = MD5.Create();
		var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < contentMD5bytes.Length; i++) {
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

		if (request.error != null) {
			GameObject.Find("MASTER_Controller").GetComponent<scr_SceneController>().DebugMessage(request.error, "error");
		}
		else {
			print ("Target replaced.");
		}

	}

	IEnumerator DeactivateTarget(string markerID) {
		string requestPath = "/targets/" + markerID;
		string serviceURI = url + requestPath;
		string httpAction = "PUT";
		string contentType = "application/json";
		string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime()); 

		PutNewTrackableRequest model = new PutNewTrackableRequest();
		model.active_flag = false;
		string requestBody = JsonUtility.ToJson(model);

		HttpWebRequest httpWReq = (HttpWebRequest) HttpWebRequest.Create(serviceURI);

		MD5 md5 = MD5.Create();
		var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < contentMD5bytes.Length; i++) {
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

		if (request.error != null) {
			GameObject.Find("MASTER_Controller").GetComponent<scr_SceneController>().DebugMessage(request.error, "error");
		}
		else {
			successfullyDeactivated = true;
			print ("Target deactivated.");
		}
	}

	IEnumerator DeleteTarget(string markerID) {
		string requestPath = "/targets/" + markerID;
		string serviceURI = url + requestPath;
		string httpAction = "DELETE";
		string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime()); 

		HttpWebRequest httpWReq = (HttpWebRequest) HttpWebRequest.Create(serviceURI);

		MD5 md5 = MD5.Create();
		var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(""));
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < contentMD5bytes.Length; i++) {
			sb.Append(contentMD5bytes[i].ToString("x2"));
		}

		string contentMD5 = sb.ToString();

		string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, "", date, requestPath);

		HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
		byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
		MemoryStream stream = new MemoryStream(sha1Bytes);
		byte[] sha1Hash = sha1.ComputeHash(stream);
		string signature = System.Convert.ToBase64String(sha1Hash);

		// Make the Web Request
		Dictionary<string, string> headers = new Dictionary<string, string> ();
		headers.Add ("host", url);
		headers.Add ("date", date);
		headers.Add ("authorization", string.Format("VWS {0}:{1}", access_key, signature));
		WWW request = new WWW(serviceURI, null, headers);
		yield return request;

		if (request.error != null) {
			GameObject.Find("MASTER_Controller").GetComponent<scr_SceneController>().DebugMessage(request.error, "error");
			successfullyDeleted = false;
		}
		else {
			GameObject.Find("MASTER_Controller").GetComponent<scr_SceneController>().DebugMessage("Marker deleted off of Vuforia cloud. ID: " + markerID);
			successfullyDeleted = true;
		}

	}
}


public class PostNewTrackableRequest
{
	public string name;
	public float  width;
	public string image;
	public string application_metadata;
}

public class PutNewTrackableRequest
{
	public string name;
	public float  width;
	public string image;
	public bool   active_flag;
	public string application_metadata;
}