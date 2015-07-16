// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
#pragma once

#include "Math.h"
#include "TMath.h"
#include "TStats.h"

using namespace System;
using namespace arma;

namespace IQ { namespace Core {namespace Math { 

	
	public ref class MathServices
	{
	public:
		generic <typename T>
			static IStats^ Stats()
			{
				return gcnew StatsService();
			}
	
		generic <typename T>
			static IVectorCalculator<T>^ VectorCalcs()
			{
				if (T::typeid == int32::typeid)
				{
					auto calc = gcnew CalculatorServiceInt32();
					return (IVectorCalculator<T>^) calc;
				}
				else
				{
					throw gcnew NotSupportedException();
				}
			}

		static IArrayCalculator^ GetArrayCalculator()
		{
			return gcnew ArrayCalculator();
		}
	};

}}}
