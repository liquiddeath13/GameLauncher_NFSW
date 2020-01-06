// dllmain.cpp : Определяет точку входа для приложения DLL.
#include "pch.h"
#include <iostream>
#include <TlHelp32.h>
#include <fstream>
#include <string>
#include <time.h>
#include <vector>

HANDLE NFSWorldProcess = nullptr;
uintptr_t NFSWorldBaseAddress = 0;
bool FinishedState = false;
std::ofstream logFile;
bool multihackDetected = false;
bool fastPowerupsHackDetected = false;
bool speedHackDetected = false;
bool smoothWallsHackDetected = false;
bool tankModeHackDetected = false;
bool wallHackDetected = false;
bool driftModeDetected = false;
DWORD processID;

uintptr_t GetModuleBaseAddress(DWORD dwProcID, LPCWSTR szModuleName)
{
	uintptr_t ModuleBaseAddress = 0;
	HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, dwProcID);
	if (hSnapshot != INVALID_HANDLE_VALUE)
	{
		MODULEENTRY32 ModuleEntry32;
		ModuleEntry32.dwSize = sizeof(MODULEENTRY32);
		if (Module32First(hSnapshot, &ModuleEntry32))
		{
			do
			{
				if (lstrcmpW(ModuleEntry32.szModule, szModuleName) == 0)
				{
					ModuleBaseAddress = (uintptr_t)ModuleEntry32.modBaseAddr;
					break;
				}
			} while (Module32Next(hSnapshot, &ModuleEntry32));
		}
		CloseHandle(hSnapshot);
	}
	return ModuleBaseAddress;
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
		NFSWorldProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, processID);
		NFSWorldBaseAddress = GetModuleBaseAddress(processID, L"nfsw.exe");
		logFile.open("anticheatlog.txt");
		logFile << "starting work" << std::endl;
		logFile << "nfs world process is " << NFSWorldProcess << std::endl;
		logFile << "nfs world base address is " << NFSWorldBaseAddress << std::endl;
		break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C" __declspec(dllexport) int NextHook(int code, WPARAM wParam, LPARAM lParam) {
	if (NFSWorldProcess != 0 && NFSWorldBaseAddress != 0) {
		char buffer[4];
		SIZE_T bytesRead = 0;
		ReadProcessMemory(NFSWorldProcess, (LPCVOID)(NFSWorldBaseAddress + 418534), buffer, sizeof(buffer), &bytesRead);
		if ((buffer[0] != '\x3B' && buffer[1] != '\x01' && buffer[1] != '\x0F' && buffer[2] != '\x84') && multihackDetected != true) {
			logFile << "detected multihack using at " << time(0) << " time stamp." << std::endl;
			multihackDetected = true;
		}
		ReadProcessMemory(NFSWorldProcess, (LPCVOID)(NFSWorldBaseAddress + 3788216), buffer, sizeof(buffer), &bytesRead);
		if ((buffer[0] != '\x80' && buffer[1] != '\x7D' && buffer[1] != '\xFB' && buffer[2] != '\x00') && fastPowerupsHackDetected != true) {
			logFile << "detected fast powerups hack using at " << time(0) << " time stamp." << std::endl;
			fastPowerupsHackDetected = true;
		}
		ReadProcessMemory(NFSWorldProcess, (LPCVOID)(NFSWorldBaseAddress + 4552702), buffer, sizeof(buffer), &bytesRead);
		if ((buffer[0] != '\x76' && buffer[1] != '\x39' && buffer[1] != '\x0F' && buffer[2] != '\x2E') && speedHackDetected != true) {
			logFile << "detected speedhack using at " << time(0) << " time stamp." << std::endl;
			speedHackDetected = true;
		}
		ReadProcessMemory(NFSWorldProcess, (LPCVOID)(NFSWorldBaseAddress + 4476396), buffer, sizeof(buffer), &bytesRead);
		if ((buffer[0] != '\x84' && buffer[1] != '\xC0' && buffer[1] != '\x0F' && buffer[2] != '\x84') && smoothWallsHackDetected != true) {
			logFile << "detected smooth walls hack using at " << time(0) << " time stamp." << std::endl;
			smoothWallsHackDetected = true;
		}
		ReadProcessMemory(NFSWorldProcess, (LPCVOID)(NFSWorldBaseAddress + 4506534), buffer, sizeof(buffer), &bytesRead);
		if ((buffer[0] != '\x74' && buffer[1] != '\x17' && buffer[1] != '\x0F' && buffer[2] != '\x57') && tankModeHackDetected != true) {
			logFile << "detected tank mode hack using at " << time(0) << " time stamp." << std::endl;
			tankModeHackDetected = true;
		}
		ReadProcessMemory(NFSWorldProcess, (LPCVOID)(NFSWorldBaseAddress + 4587060), buffer, sizeof(buffer), &bytesRead);
		if ((buffer[0] != '\x74' && buffer[1] != '\x22' && buffer[1] != '\x8B' && buffer[2] != '\x16') && wallHackDetected != true) {
			logFile << "detected wallhack using at " << time(0) << " time stamp." << std::endl;
			wallHackDetected = true;
		}
		ReadProcessMemory(NFSWorldProcess, (LPCVOID)(NFSWorldBaseAddress + 4486168), buffer, sizeof(buffer), &bytesRead);
		if (buffer[0] != '\xF3' && buffer[1] != '\x0F' && buffer[1] != '\x10' && buffer[2] != '\x86') {
			if (buffer[0] == '\xE8' && multihackDetected != true) {
				logFile << "detected multihack using at " << time(0) << " time stamp." << std::endl;
				multihackDetected = true;
			}
			if (buffer[0] == '\xE9' && driftModeDetected != true) {
				logFile << "detected driftmode using at " << time(0) << " time stamp." << std::endl;
				driftModeDetected = true;
			}
		}
	}
	return CallNextHookEx(NULL, code, wParam, lParam);
}