
#include "stdafx.h"

#include "strutil.h"
#include "fileutil.h"
#include <string>

#define BUF_LEN 1024


static const std::string base64_chars = 
	"ABCDEFGHIJKLMNOPQRSTUVWXYZ"
	"abcdefghijklmnopqrstuvwxyz"
	"0123456789+/";

static inline bool is_base64(unsigned char c) {
  return (isalnum(c) || (c == '+') || (c == '/'));
}


std::string base64_encode(unsigned char const* bytes_to_encode, unsigned int in_len) {
	std::string ret;
	int i = 0;
	int j = 0;
	unsigned char char_array_3[3];
	unsigned char char_array_4[4];

	while (in_len--) {
		char_array_3[i++] = *(bytes_to_encode++);
		if (i == 3) {
			char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
			char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
			char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
			char_array_4[3] = char_array_3[2] & 0x3f;

			for(i = 0; (i <4) ; i++)
				ret += base64_chars[char_array_4[i]];
			i = 0;
		}
	}

	if (i)
	{
		for(j = i; j < 3; j++)
			char_array_3[j] = '\0';

		char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
		char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
		char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
		char_array_4[3] = char_array_3[2] & 0x3f;

		for (j = 0; (j < i + 1); j++)
			ret += base64_chars[char_array_4[j]];

		while((i++ < 3))
			ret += '=';

	}

	return ret;
}

std::string base64_decode(std::string const& encoded_string) {
  int in_len = encoded_string.size();
  int i = 0;
  int j = 0;
  int in_ = 0;
  unsigned char char_array_4[4], char_array_3[3];
  std::string ret;

  while (in_len-- && ( encoded_string[in_] != '=') && is_base64(encoded_string[in_])) {
    char_array_4[i++] = encoded_string[in_]; in_++;
    if (i ==4) {
      for (i = 0; i <4; i++)
        char_array_4[i] = base64_chars.find(char_array_4[i]);

      char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
      char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
      char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

      for (i = 0; (i < 3); i++)
        ret += char_array_3[i];
      i = 0;
    }
  }

  if (i) {
    for (j = i; j <4; j++)
      char_array_4[j] = 0;

    for (j = 0; j <4; j++)
      char_array_4[j] = base64_chars.find(char_array_4[j]);

    char_array_3[0] = (char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4);
    char_array_3[1] = ((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2);
    char_array_3[2] = ((char_array_4[2] & 0x3) << 6) + char_array_4[3];

    for (j = 0; (j < i - 1); j++) ret += char_array_3[j];
  }

  return ret;
}


static LPWSTR CustomBreakDownCAData(
	__inout LPWSTR* ppwzData
	)
{
	if (!ppwzData)
		return NULL;
	if (0 == *ppwzData)
		return NULL;

	WCHAR delim[] = { 124 , 0 };   // that would be "|" and null terminated string

	LPWSTR pwzReturn = *ppwzData;
	LPWSTR pwz = wcsstr(pwzReturn, delim);
	if (pwz)
	{
		*pwz = 0;
		*ppwzData = pwz + 1;
	}
	else
		*ppwzData = 0;

	return pwzReturn;
}


HRESULT ReadStringFromCAData(
	__deref_in LPWSTR* ppwzCustomActionData,
	__deref_out_z LPWSTR* ppwzString
	)
{
	HRESULT hr = S_OK;

	LPCWSTR pwz = CustomBreakDownCAData(ppwzCustomActionData);
	if (!pwz)
		return E_NOMOREITEMS;

	hr = StrAllocString(ppwzString, pwz, 0);
	ExitOnFailure(hr, "failed to allocate memory for string");

	hr  = S_OK;
LExit:
	return hr;
}


HRESULT ReadIntegerFromCAData(
	__deref_in LPWSTR* ppwzCustomActionData,
	__out int* piResult
	)
{
	LPCWSTR pwz = CustomBreakDownCAData(ppwzCustomActionData);
	if (!pwz || 0 == wcslen(pwz))
		return E_NOMOREITEMS;

	*piResult = wcstol(pwz, NULL, 10);
	return S_OK;
}


UINT __stdcall UpdateApacheConfig(MSIHANDLE hInstall)
{
	HRESULT hr = S_OK;
	UINT er = ERROR_SUCCESS;

	hr = WcaInitialize(hInstall, "UpdateApacheConfig");
	ExitOnFailure(hr, "Failed to initialize");

	WcaLog(LOGMSG_STANDARD, "UpdateApacheConfig initialized.");

	// TODO: Add your custom action code here.


	LPWSTR pwzData = NULL;
	LPWSTR pwz = NULL;
	LPWSTR pwzSuiteInstallDir = NULL;
	LPWSTR pwzApachePort = NULL;
	LPWSTR pwzApacheConfFile = NULL;
	LPWSTR pwzApacheConfContent = NULL;
	FILE_ENCODING pfeEncoding;

	hr = WcaGetProperty( L"CustomActionData", &pwzData);
	ExitOnFailure(hr, "failed to get CustomActionData");

	pwz = pwzData;

	hr = ReadStringFromCAData(&pwz, &pwzSuiteInstallDir);
	ExitOnFailure(hr, "failed to read pwzSuiteInstallDir from custom action data: %ls", pwz);

	hr = ReadStringFromCAData(&pwz, &pwzApachePort);
	ExitOnFailure(hr, "failed to read pwzApachePort from custom action data: %ls", pwz);

	hr = ReadStringFromCAData(&pwz, &pwzApacheConfFile);
	ExitOnFailure(hr, "failed to read SQL User from custom action data: %ls", pwz);

	FileToString(pwzApacheConfFile, &pwzApacheConfContent, &pfeEncoding);
	StrReplaceStringAll(&pwzApacheConfContent, L"C:\\SuiteDir\\", pwzSuiteInstallDir);
	StrReplaceStringAll(&pwzApacheConfContent, L"C:/SuiteDir/", pwzSuiteInstallDir);
	StrReplaceStringAll(&pwzApacheConfContent, L"8888", pwzApachePort);

	FileFromString(pwzApacheConfFile, 0, pwzApacheConfContent, pfeEncoding);

	WcaLog(LOGMSG_STANDARD, "UpdateApacheConfig completed.");
LExit:
	er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
	return WcaFinalize(er);
}



UINT __stdcall UpdatePHPMyAdminConfig(MSIHANDLE hInstall)
{
	HRESULT hr = S_OK;
	UINT er = ERROR_SUCCESS;

	hr = WcaInitialize(hInstall, "UpdatePHPMyAdminConfig");
	ExitOnFailure(hr, "Failed to initialize");

	WcaLog(LOGMSG_STANDARD, "UpdatePHPMyAdminConfig initialized.");

	LPWSTR pwzData = NULL;
	LPWSTR pwz = NULL;
	LPWSTR pwzSuiteInstallDir = NULL;
	LPWSTR pwzMySQLPort = NULL;
	LPWSTR pwzPhpMyAdminConfFile = NULL;
	LPWSTR pwzPhpMyAdminConfContent = NULL;
	FILE_ENCODING pfeEncoding;

	hr = WcaGetProperty( L"CustomActionData", &pwzData);
	ExitOnFailure(hr, "failed to get CustomActionData");

	pwz = pwzData;

	hr = ReadStringFromCAData(&pwz, &pwzMySQLPort);
	ExitOnFailure(hr, "failed to read pwzMySQLPort from custom action data: %ls", pwz);

	hr = ReadStringFromCAData(&pwz, &pwzPhpMyAdminConfFile);
	ExitOnFailure(hr, "failed to read pwzPhpMyAdminConfFile from custom action data: %ls", pwz);

	FileToString(pwzPhpMyAdminConfFile, &pwzPhpMyAdminConfContent, &pfeEncoding);
	StrReplaceStringAll(&pwzPhpMyAdminConfContent, L"3333", pwzMySQLPort);

	FileFromString(pwzPhpMyAdminConfFile, 0, pwzPhpMyAdminConfContent, pfeEncoding);

	WcaLog(LOGMSG_STANDARD, "UpdatePHPMyAdminConfig completed.");
LExit:
	er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
	return WcaFinalize(er);
}



UINT __stdcall UpdateWebConfig(MSIHANDLE hInstall)
{
	HRESULT hr = S_OK;
	UINT er = ERROR_SUCCESS;

	hr = WcaInitialize(hInstall, "UpdateWebConfig");
	ExitOnFailure(hr, "Failed to initialize");

	WcaLog(LOGMSG_STANDARD, "UpdateWebConfig initialized.");

	LPWSTR pwzData = NULL;
	LPWSTR pwz = NULL;
	LPWSTR pwzMySQLPort = NULL;
	LPWSTR pwzMySQLPass = NULL;
	LPWSTR pwzCfgFile = NULL;
	LPWSTR pwzCfgContent = NULL;
	FILE_ENCODING pfeEncoding;

	hr = WcaGetProperty( L"CustomActionData", &pwzData);
	ExitOnFailure(hr, "failed to get CustomActionData");

	pwz = pwzData;

	hr = ReadStringFromCAData(&pwz, &pwzMySQLPort);
	ExitOnFailure(hr, "failed to read pwzMySQLPort from custom action data: %ls", pwz);

	hr = ReadStringFromCAData(&pwz, &pwzMySQLPass);
	ExitOnFailure(hr, "failed to read pwzMySQLPass from custom action data: %ls", pwz);

	hr = ReadStringFromCAData(&pwz, &pwzCfgFile);
	ExitOnFailure(hr, "failed to read pwzCfgFile from custom action data: %ls", pwz);

	FileToString(pwzCfgFile, &pwzCfgContent, &pfeEncoding);
	StrReplaceStringAll(&pwzCfgContent, L"3333", pwzMySQLPort);
	StrReplaceStringAll(&pwzCfgContent, L"rootpass", pwzMySQLPass);

	FileFromString(pwzCfgFile, 0, pwzCfgContent, pfeEncoding);

	WcaLog(LOGMSG_STANDARD, "UpdateWebConfig completed.");
LExit:
	er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
	return WcaFinalize(er);
}


UINT __stdcall UpdateRedisConfig(MSIHANDLE hInstall)
{
	HRESULT hr = S_OK;
	UINT er = ERROR_SUCCESS;

	hr = WcaInitialize(hInstall, "UpdateRedisConfig");
	ExitOnFailure(hr, "Failed to initialize");

	WcaLog(LOGMSG_STANDARD, "UpdateRedisConfig initialized.");

	LPWSTR pwzData = NULL;
	LPWSTR pwz = NULL;
	LPWSTR pwzRedisPort = NULL;
	LPWSTR pwzCfgFile = NULL;
	LPWSTR pwzCfgContent = NULL;
	FILE_ENCODING pfeEncoding;

	hr = WcaGetProperty( L"CustomActionData", &pwzData);
	ExitOnFailure(hr, "failed to get CustomActionData");

	pwz = pwzData;

	hr = ReadStringFromCAData(&pwz, &pwzRedisPort);
	ExitOnFailure(hr, "failed to read pwzRedisPort from custom action data: %ls", pwz);

	hr = ReadStringFromCAData(&pwz, &pwzCfgFile);
	ExitOnFailure(hr, "failed to read pwzCfgFile from custom action data: %ls", pwz);

	FileToString(pwzCfgFile, &pwzCfgContent, &pfeEncoding);
	StrReplaceStringAll(&pwzCfgContent, L"6888", pwzRedisPort);

	FileFromString(pwzCfgFile, 0, pwzCfgContent, pfeEncoding);

	WcaLog(LOGMSG_STANDARD, "UpdateRedisConfig completed.");
LExit:
	er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
	return WcaFinalize(er);
}

UINT __stdcall UpdateWebsocketConfig(MSIHANDLE hInstall)
{
	HRESULT hr = S_OK;
	UINT er = ERROR_SUCCESS;

	hr = WcaInitialize(hInstall, "UpdateWebsocketConfig");
	ExitOnFailure(hr, "Failed to initialize");

	WcaLog(LOGMSG_STANDARD, "UpdateWebsocketConfig initialized.");

	LPWSTR pwzData = NULL;
	LPWSTR pwz = NULL;
	LPWSTR pwzWebsocketPort = NULL;
	LPWSTR pwzCfgFile = NULL;
	LPWSTR pwzCfgContent = NULL;
	FILE_ENCODING pfeEncoding;

	hr = WcaGetProperty( L"CustomActionData", &pwzData);
	ExitOnFailure(hr, "failed to get CustomActionData");

	pwz = pwzData;

	hr = ReadStringFromCAData(&pwz, &pwzWebsocketPort);
	ExitOnFailure(hr, "failed to read pwzWebsocketPort from custom action data: %ls", pwz);

	hr = ReadStringFromCAData(&pwz, &pwzCfgFile);
	ExitOnFailure(hr, "failed to read pwzCfgFile from custom action data: %ls", pwz);

	FileToString(pwzCfgFile, &pwzCfgContent, &pfeEncoding);
	StrReplaceStringAll(&pwzCfgContent, L"3088", pwzWebsocketPort);

	FileFromString(pwzCfgFile, 0, pwzCfgContent, pfeEncoding);

	WcaLog(LOGMSG_STANDARD, "UpdateWebsocketConfig completed.");
LExit:
	er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
	return WcaFinalize(er);
}

UINT __stdcall EncodePassword(MSIHANDLE hInstall)				
{
	HRESULT hr = S_OK;
	UINT er = ERROR_SUCCESS;

	wchar_t wzPass[BUF_LEN] = L"";
	DWORD nPasssLen = BUF_LEN;

	hr = WcaInitialize(hInstall, "EncodePassword");
	ExitOnFailure(hr, "Failed to initialize EncodePassword");

	WcaLog(LOGMSG_STANDARD, "EncodePassword Initialized.");

	hr = MsiGetProperty(hInstall, L"MYSQL_ROOT_PASS", wzPass, &nPasssLen);
	ExitOnFailure(hr, "Failed to get MYSQL_ROOT_PASS property");

	if (SUCCEEDED(hr))
	{
		wchar_t wzEncPass[BUF_LEN] = L"";
		std::wstring wsPass;
		std::string sPass, sEncPass;
		size_t n;

		wsPass = wzPass;
		sPass.resize(wsPass.size());
		std::copy(wsPass.begin(), wsPass.end(), sPass.begin());
		sEncPass = base64_encode(reinterpret_cast<const unsigned char*>(sPass.c_str()), sPass.length());
		mbstowcs_s(&n, wzEncPass, BUF_LEN, sEncPass.c_str(), BUF_LEN);
		WcaLog(LOGMSG_STANDARD, "Using encoded password: %S", wzEncPass);

		hr = MsiSetProperty(hInstall, L"MYSQL_ROOT_ENCPASS", wzEncPass);
		ExitOnFailure(hr, "Failed to set MYSQL_ROOT_ENCPASS property");
	}

	WcaLog(LOGMSG_STANDARD, "EncodePassword done.");

LExit:
	er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
	return WcaFinalize(er);
}


UINT __stdcall DecodePassword(MSIHANDLE hInstall)				
{
	HRESULT hr = S_OK;
	UINT er = ERROR_SUCCESS;

	wchar_t wzEncPass[BUF_LEN] = L"";
	DWORD nPasssLen = BUF_LEN;

	hr = WcaInitialize(hInstall, "DecodePassword");
	ExitOnFailure(hr, "Failed to initialize DecodePassword");

	WcaLog(LOGMSG_STANDARD, "DecodePassword Initialized.");

	hr = MsiGetProperty(hInstall, L"MYSQL_ROOT_ENCPASS", wzEncPass, &nPasssLen);
	ExitOnFailure(hr, "Failed to get MYSQL_ROOT_ENCPASS property");

	if (SUCCEEDED(hr))
	{
		wchar_t wzPass[BUF_LEN] = L"";
		std::wstring wsEncPass;
		std::string sEncPass, sPass;
		size_t n;

		wsEncPass = wzEncPass;
		sEncPass.resize(wsEncPass.size());
		std::copy(wsEncPass.begin(), wsEncPass.end(), sEncPass.begin());
		sPass = base64_decode(sEncPass.c_str());
		mbstowcs_s(&n, wzPass, BUF_LEN, sPass.c_str(), BUF_LEN);

		hr = MsiSetProperty(hInstall, L"MYSQL_ROOT_PASS", wzPass);
		ExitOnFailure(hr, "Failed to set MYSQL_ROOT_PASS property");
	}

	WcaLog(LOGMSG_STANDARD, "DecodePassword done.");

LExit:
	er = SUCCEEDED(hr) ? ERROR_SUCCESS : ERROR_INSTALL_FAILURE;
	return WcaFinalize(er);
}


// DllMain - Initialize and cleanup WiX custom action utils.
extern "C" BOOL WINAPI DllMain(
	__in HINSTANCE hInst,
	__in ULONG ulReason,
	__in LPVOID
	)
{
	switch(ulReason)
	{
	case DLL_PROCESS_ATTACH:
		WcaGlobalInitialize(hInst);
		break;

	case DLL_PROCESS_DETACH:
		WcaGlobalFinalize();
		break;
	}

	return TRUE;
}
