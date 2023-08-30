#include <iostream>
#include <fstream>
#include <vector>
#include <map>

#include <Windows.h>
#include <compressapi.h>

#pragma comment(lib, "Cabinet.lib")

const char* algoNames[] = {
	"(invalid)",
	"(null)",
	"COMPRESS_ALGORITHM_MSZIP",
	"COMPRESS_ALGORITHM_XPRESS",
	"COMPRESS_ALGORITHM_XPRESS_HUFF",
	"COMPRESS_ALGORITHM_LZMS"
};

int main(int argc, const char* argv[])
{
	if (argc != 3) {
		std::cout << "usage: " << argv[0] << " <shader source file> <output file>" << std::endl;
		return 1;
	}

	std::ifstream inFile(argv[1], std::ios::binary | std::ios::ate);
	if (!inFile.good()) {
		std::cout << "error: could not open input file '" << argv[1] << "'" << std::endl;
		return 1;
	}

	auto inSize = inFile.tellg();
	inFile.seekg(0, std::ios::beg);

	std::vector<char> inBuffer(inSize);
	inFile.read(inBuffer.data(), inSize);
	inFile.close();

	std::vector<char> outBuffer(inSize * 2);
	std::vector<char> finalBuffer(outBuffer.size());

	DWORD bestAlgo;
	SIZE_T bestSize = SIZE_MAX;
	for (DWORD algo = 2; algo < COMPRESS_ALGORITHM_MAX; algo++)
	{
		COMPRESSOR_HANDLE hCompressor;
		SIZE_T currentSize;
		BOOL success;

		success = CreateCompressor(algo, nullptr, &hCompressor);
		if (!success) {
			std::cout << "warning: could not create compressor " << algoNames[algo] << ", error " << GetLastError() << std::endl;
			continue;
		}

		success = Compress(
			hCompressor,
			inBuffer.data(), inBuffer.size(),
			outBuffer.data(), outBuffer.size(),
			&currentSize
		);
		if (!success) {
			std::cout << "warning: could not compress using " << algoNames[algo] << ", error " << GetLastError() << std::endl;
			continue;
		}

		if (currentSize < bestSize) {
			bestSize = currentSize;
			bestAlgo = algo;
			memcpy(finalBuffer.data(), outBuffer.data(), bestSize);
		}
	}

	std::cout << "Algorithm: " << algoNames[bestAlgo] << std::endl;
	std::cout << "Compressed size: " << bestSize << std::endl;
	std::cout << "Original size: " << inBuffer.size() << std::endl;
	std::cout << "Compression ratio: " << (bestSize / (float)inBuffer.size()) << std::endl;

	std::ofstream outFile(argv[2]);
	if (!outFile.good()) {
		std::cout << "error: could not open output file '" << argv[2] << "'" << std::endl;
		return 1;
	}

	for (size_t i = 0; i < bestSize; i++)
	{
		outFile << "0x" << std::hex << (uint32_t)(uint8_t)(finalBuffer[i]);
		if (i != bestSize - 1)
			outFile << ", ";

		if (i % 32 == 0 && i != 0)
			outFile << std::endl;
	}

	outFile.close();
	std::cout << "Output written to '" << argv[2] << "'.";
	return 0;
}
