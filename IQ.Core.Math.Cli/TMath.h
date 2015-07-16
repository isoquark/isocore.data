// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
#pragma once

#include "Math.h"

using namespace System;

namespace IQ { namespace Core { namespace Math {
	
	class TMath
	{
	public:
		template <typename T> static T DotProduct(Vector<T>^ x, Vector<T>^ y)
		{
			auto xcomp = x->Components;
			pin_ptr<T> pX = &xcomp[0];
			Row<T> row(pX, xcomp->Length, false, true);

			auto ycomp = y->Components;
			pin_ptr<T> pY = &ycomp[0];
			Col<T> col(pY, ycomp->Length, false, true);

			return as_scalar(row*col);
		}

		template <typename T> static T DotProduct2(Vector<T>^ x, Vector<T>^ y)
		{
			T result = 0;
			for (int i = 0; i < x->Components->Length; i++)
			{
				result += (x->Components[i] * y->Components[i]);
			}
			return result;
		}

		template <typename T> static array<T>^ MultiplyArrays(array<T>^ x, array<T>^ y)
		{
			if (x->Length != y->Length)
			{
				throw gcnew ArgumentException("Arrays must have the same length");
			}
			auto result = gcnew array<T>(x->Length);
			for (auto i = 0; i < x->Length; i++)
			{
				result[i] = x[i] * y[i];
			}
			return result;
		}

	};

	private ref class CalculatorServiceInt32 : public  IVectorCalculator<int32>
	{

	public:
		virtual int Dot(Vector<int>^ x, Vector<int>^ y)
		{
			return TMath::DotProduct2(x, y);
		}
	};

	private ref class ArrayCalculator : public IArrayCalculator
	{

	public:
		generic <typename T>
			virtual array<T>^ Multiply(array<T>^ x, array<T>^ y)
			{
				if (T::typeid == int32::typeid)
				{
					auto result = TMath::MultiplyArrays<int>((array<int>^)x, (array<int>^)y);
					return (array<T>^) result;
				}
				else
					throw gcnew NotImplementedException();
			}

	};

}}}