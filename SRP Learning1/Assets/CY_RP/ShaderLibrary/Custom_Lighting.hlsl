﻿#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
float3 GetLighting (Surface surface) {
	return GetLighting(surface, GetDirectionalLight());
}
#endif