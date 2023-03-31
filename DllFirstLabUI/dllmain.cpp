// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <mkl_df.h>
#include <iostream>

BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}


extern "C" _declspec(dllexport)
int interpolate(int numberOfInterpolationPoints,
	double* interpolationPoints, double* forceValuesInInterpolationPoints,
	double* firstDerivativeOnSegmentEnds,
	int numberOfOutputPoints, double* outputPointsSegmentEnds,
	double* leftIntegralEnds, double* rightIntegralEnds,
	double* forceValuesInOutputPoints, double* integralValues) {

	DFTaskPtr task;
	int error_code = dfdNewTask1D(&task, numberOfInterpolationPoints, interpolationPoints, DF_NON_UNIFORM_PARTITION, 1, forceValuesInInterpolationPoints, DF_MATRIX_STORAGE_ROWS);
	if (error_code != DF_STATUS_OK) {
		return 1;
	}

	double* coeff = new double[1 * DF_PP_CUBIC * (numberOfInterpolationPoints - 1)];
	error_code = dfdEditPPSpline1D(task,
		DF_PP_CUBIC, DF_PP_NATURAL,
		DF_BC_1ST_LEFT_DER | DF_BC_1ST_RIGHT_DER,
		firstDerivativeOnSegmentEnds,
		DF_NO_IC, NULL, coeff,
		DF_NO_HINT);
	if (error_code != DF_STATUS_OK) {
		dfDeleteTask(&task);
		delete[] coeff;
		return 2;
	}
	delete[] coeff;

	error_code = dfdConstruct1D(task, DF_PP_SPLINE, DF_METHOD_STD);
	if (error_code != DF_STATUS_OK) {
		dfDeleteTask(&task);
		return 3;
	}

	const int dorder[3] = { 1, 1, 1 };
	error_code = dfdInterpolate1D(task,
		DF_INTERP, DF_METHOD_PP,
		numberOfOutputPoints, outputPointsSegmentEnds, DF_UNIFORM_PARTITION,
		3, dorder, NULL, forceValuesInOutputPoints,
		DF_MATRIX_STORAGE_ROWS, NULL);
	if (error_code != DF_STATUS_OK) {
		dfDeleteTask(&task);
		return 4;
	}

	error_code = dfdIntegrate1D(task,
		DF_METHOD_PP, 1,
		leftIntegralEnds, DF_SORTED_DATA,
		rightIntegralEnds, DF_SORTED_DATA,
		NULL, NULL, integralValues, DF_MATRIX_STORAGE_ROWS);
	if (error_code != DF_STATUS_OK) {
		dfDeleteTask(&task);
		return 5;
	}

	return error_code;
}


/*
extern "C" _declspec(dllexport)
int interpolate(int amount) {
	std::cout << amount;
	return 0;
}
*/
//if (error_code == DF_ERROR_BAD_NX) {
//	return -1;
//}
//else if (error_code == DF_ERROR_BAD_X) {
//	return -2;
//}
//else if (error_code == DF_ERROR_BAD_X_HINT) {
//	return -3;
//}
//else if (error_code == DF_ERROR_BAD_NY) {
//	return -4;
//}
//else if (error_code == DF_ERROR_BAD_Y) {
//	return -5;
//}
//else if (error_code == DF_ERROR_BAD_Y_HINT) {
//	return -6;
//}