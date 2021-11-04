// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <string>
#include <iostream>
#include <filesystem>
using namespace std;
namespace fs = std::filesystem;

inline bool ends_with(std::string const& value, std::string const& ending)
{
	if (ending.size() > value.size()) return false;
	return std::equal(ending.rbegin(), ending.rend(), value.rbegin());
}

string patches_dir = "Patches";
string patch_ext = ".Patch.dll";

DWORD WINAPI LoadPatches(LPVOID param)
{
	Sleep(1000);
	SetDllDirectoryW(L"Patches");
	for (const auto& entry : fs::recursive_directory_iterator(patches_dir))
	{
		auto p = entry.path().string();
		if (ends_with(p, patch_ext))
		{
			cout << "[PatchLoader] Loading patch " + p << endl;
			auto handle = LoadLibraryW(entry.path().wstring().c_str());
			if (handle)
			{
				auto pMain = GetProcAddress(handle, "LoadEditorPatch");
				if (pMain)
				{
					cout << "[PatchLoader] Initializing patch " + p << endl;
					pMain();
				}
				else
				{
					cout << "[PatchLoader] LoadEditorPatch not found in patch." << endl;
				}
			}
			else
			{
				cout << "[PatchLoader] Failed to load patch " + p << endl;
			}
		}
	}
	return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  fdwReason,
	LPVOID lpReserved
)
{
	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
	{
		CreateThread(NULL, 0, &LoadPatches, NULL, 0, NULL);
	}
	break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

