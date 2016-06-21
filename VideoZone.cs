using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class VideoZone {
	public string zoneName = "";
	public string zoneId = "";
	public VideoZoneType zoneType = VideoZoneType.None;
	
	public VideoZone(string newZoneName, string newZoneId, VideoZoneType newVideoZoneType) {
		zoneName = newZoneName;
		zoneId = newZoneId;
		zoneType = newVideoZoneType;
	}
}
