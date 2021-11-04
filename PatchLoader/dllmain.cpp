// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <string>
#include <iostream>
#include <filesystem>
using namespace std;
namespace fs = std::filesystem;

void Print(auto arg)
{
#if DEBUG
	cout << arg << endl;
#endif
}


bool ends_with(string const& value, string const& ending)
{
	if (ending.size() > value.size()) return false;
	return equal(ending.rbegin(), ending.rend(), value.rbegin());
}

const string patches_dir = "Patches";
const string patch_ext = ".Patch.dll";

DWORD WINAPI LoadPatches(LPVOID param)
{
	Sleep(1000);
	SetDllDirectoryW(L"Patches");
	for (const auto& entry : fs::recursive_directory_iterator(patches_dir))
	{
		auto p = entry.path().string();
		if (ends_with(p, patch_ext))
		{
			Print("[PatchLoader] Loading patch " + p);
			auto handle = LoadLibraryW(entry.path().wstring().c_str());
			if (handle)
			{
				auto pMain = GetProcAddress(handle, "LoadEditorPatch");
				if (pMain)
				{
					Print("[PatchLoader] Initializing patch " + p);
					pMain();
				}
				else
				{
					Print("[PatchLoader] LoadEditorPatch not found in patch.");
				}
			}
			else
			{
				Print("[PatchLoader] Failed to load patch " + p);
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

