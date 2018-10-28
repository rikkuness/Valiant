const float PI = 3.14159;
//HSV <-> RGB
float3 HUEtoRGB(in float H)
{
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);
	return saturate(float3(R, G, B));
}

float Epsilon = 1e-10;

float3 RGBtoHCV(in float3 RGB)
{
	// Based on work by Sam Hocevar and Emil Persson
	float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
	float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
	float C = Q.x - min(Q.w, Q.y);
	float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
	return float3(H, C, Q.x);
}
float3 RGBtoHSV(in float3 RGB)
{
	float3 HCV = RGBtoHCV(RGB);
	float S = HCV.y / (HCV.z + Epsilon);
	return float3(HCV.x, S, HCV.z);
}
float3 HSVtoRGB(in float3 HSV)
{
	float3 RGB = HUEtoRGB(HSV.x);
	return ((RGB - 1) * HSV.y + 1) * HSV.z;
}


float3 RGB_to_HSV(float3 RGB)
{
	return RGBtoHSV(RGB);
}


float3 HSV_to_RGB(float3 HSV)
{
	return HSVtoRGB(HSV);
}


//Document here (http://www.easyrgb.com/en/math.php#text2)
float3 RGBtoXYZ(in float3 RGB) {
	float3 var_XYZ;

	var_XYZ.x = (RGB.r > 0.04045) ? pow(abs((RGB.r + 0.055) / 1.055), 2.4) : RGB.r / 12.92;
	var_XYZ.y = (RGB.g > 0.04045) ? pow(abs((RGB.g + 0.055) / 1.055), 2.4) : RGB.g / 12.92;
	var_XYZ.z = (RGB.b > 0.04045) ? pow(abs((RGB.b + 0.055) / 1.055), 2.4) : RGB.b / 12.92;

	return float3(var_XYZ.x * 41.24 + var_XYZ.y * 35.76 + var_XYZ.z * 18.05, var_XYZ.x * 21.26 + var_XYZ.y * 71.52 + var_XYZ.z * 7.22, var_XYZ.x * 1.93 + var_XYZ.y * 11.92 + var_XYZ.z * 95.05);
}

float3 XYZtoLAB(in float3 XYZ) {
	//10 degree(CIE 1964) Daylight, sRGB, Adobe-RGB
	float3 ref_XYZ = XYZ / float3(94.811, 100, 107.304);

	float var_X = (ref_XYZ.x > 0.008856) ? pow(abs(ref_XYZ.x), 1.0 / 3.0) : (7.787 * ref_XYZ.x) + (16.0 / 116.0);
	float var_Y = (ref_XYZ.y > 0.008856) ? pow(abs(ref_XYZ.y), 1.0 / 3.0) : (7.787 * ref_XYZ.y) + (16.0 / 116.0);
	float var_Z = (ref_XYZ.z > 0.008856) ? pow(abs(ref_XYZ.z), 1.0 / 3.0) : (7.787 * ref_XYZ.z) + (16.0 / 116.0);
	return float3((116.0 * var_Y) - 16.0, 500.0 * (var_X - var_Y), 200.0 * (var_Y - var_Z));
}

float3 RGBtoLAB(in float3 RGB) {
	float3 lab = XYZtoLAB(RGBtoXYZ(RGB));
	//Normalize to 0-1 range
	return float3(lab.x / 100.0, 0.5 + 0.5 * (lab.y / 127.0), 0.5 + 0.5 * (lab.z / 127.0));
}

float3 RGB_to_LAB(float3 RGB) {
	return RGBtoLAB(RGB);
}

float3 LABtoXYZ(in float3 LAB) {
	float fy = (LAB.x + 16.0) / 116.0;
	float fx = LAB.y / 500.0 + fy;
	float fz = fy - LAB.z / 200.0;
	//10 degree(CIE 1964) Daylight, sRGB, Adobe-RGB
	return float3(
		94.811 * ((fx > 0.206897) ? fx * fx * fx : (fx - 16.0 / 116.0) / 7.787),
		100.000 * ((fy > 0.206897) ? fy * fy * fy : (fy - 16.0 / 116.0) / 7.787),
		107.304 * ((fz > 0.206897) ? fz * fz * fz : (fz - 16.0 / 116.0) / 7.787)
		);
}

float3 XYZtoRGB(in float3 XYZ) {
	float var_X = XYZ.x / 100.0;
	float var_Y = XYZ.y / 100.0;
	float var_Z = XYZ.z / 100.0;

	float3 var_RGB = float3(var_X * 3.2406 + var_Y * -1.5372 + var_Z * -0.4986, var_X * -0.9689 + var_Y * 1.8758 + var_Z * 0.0415, var_X * 0.0557 + var_Y * -0.2040 + var_Z * 1.0570);

	float3 RGB;
	RGB.x = (var_RGB.r > 0.0031308) ? ((1.055 * pow(var_RGB.r, (1.0 / 2.4))) - 0.055) : 12.92 * var_RGB.r;
	RGB.y = (var_RGB.g > 0.0031308) ? ((1.055 * pow(var_RGB.g, (1.0 / 2.4))) - 0.055) : 12.92 * var_RGB.g;
	RGB.z = (var_RGB.b > 0.0031308) ? ((1.055 * pow(var_RGB.b, (1.0 / 2.4))) - 0.055) : 12.92 * var_RGB.b;
	return RGB;
}

float3 LABtoRGB(in float3 LAB) {
	//Convert from normalized value back to standard (L:0-100, A:-127-128, B:-127-128)
	float3 var_LAB = float3(100.0 * LAB.x, 2.0 * 127.0 * (LAB.y - 0.5), 2.0 * 127.0 * (LAB.z - 0.5));
	return XYZtoRGB(LABtoXYZ(var_LAB));
}

float3 LAB_to_RGB(float3 LAB) {
	return LABtoRGB(LAB);
}

float LABDistance(float3 labColor1, float3 labColor2) {
	//Convert from normalized value back to standard (L:0-100, A:-127-128, B:-127-128)
	float3 var_LAB1 = float3(100.0 * labColor1.x, 2.0 * 127.0 * (labColor1.y - 0.5), 2.0 * 127.0 * (labColor1.z - 0.5));
	float3 var_LAB2 = float3(100.0 * labColor2.x, 2.0 * 127.0 * (labColor2.y - 0.5), 2.0 * 127.0 * (labColor2.z - 0.5));
	float3 var_dis = float3(var_LAB1.x - var_LAB2.x, var_LAB1.y - var_LAB2.y, var_LAB1.z - var_LAB2.z);
	float distance = sqrt(var_dis.x*var_dis.x + var_dis.y*var_dis.y + var_dis.z*var_dis.z) / 100.0; //Range is from 0-375, treat distance over 100 all to be the biggest value. 
	return saturate(distance);
}