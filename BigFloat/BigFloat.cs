namespace System.Numerics
{
	public struct BigFloat
	{
		/// <summary>
		/// The maximum Radix value for division operations.
		/// </summary>
		const int divmax = 1000;
		const int sqrtmax = divmax - 1;
		const int expmax = divmax - 1;
		const int logmax = 10;
		const int posinf = -1;
		const int neginf = -2;
		const int nan = -3;

		private BigInteger Value;
		private int Radix;

		private BigFloat(BigInteger value, int radix)
		{
			Value = value;
			Radix = radix;
			while (Radix > 0)
			{
				if (Value % 10 != 0)
					break;
				Value /= 10;
				Radix--;
			}
		}

		public BigFloat(params byte[] data)
		{
			Radix = BitConverter.ToInt32(data, 0);
			byte[] d2 = new byte[data.Length - sizeof(int)];
			Array.Copy(data, sizeof(int), d2, 0, d2.Length);
			Value = new BigInteger(d2);
		}

		public BigFloat(double value)
		{
			if (double.IsNaN(value))
			{
				Value = BigInteger.Zero;
				Radix = nan;
			}
			else if (double.IsNegativeInfinity(value))
			{
				Value = BigInteger.Zero;
				Radix = neginf;
			}
			else if (double.IsPositiveInfinity(value))
			{
				Value = BigInteger.Zero;
				Radix = posinf;
			}
			else
			{
				string str = ToLongString(value);
				if (!str.Contains("."))
				{
					Value = BigInteger.Parse(str);
					Radix = 0;
				}
				else
				{
					string[] split = str.Split('.');
					Value = BigInteger.Parse(split[0] + split[1]);
					Radix = split[1].Length;
				}
			}
		}

		public BigFloat(float value)
		{
			if (float.IsNaN(value))
			{
				Value = BigInteger.Zero;
				Radix = nan;
			}
			else if (float.IsNegativeInfinity(value))
			{
				Value = BigInteger.Zero;
				Radix = neginf;
			}
			else if (float.IsPositiveInfinity(value))
			{
				Value = BigInteger.Zero;
				Radix = posinf;
			}
			else
			{
				string str = ToLongString(value);
				if (!str.Contains("."))
				{
					Value = BigInteger.Parse(str);
					Radix = 0;
				}
				else
				{
					string[] split = str.Split('.');
					Value = BigInteger.Parse(split[0] + split[1]);
					Radix = split[1].Length;
				}
			}
		}

		public BigFloat(decimal value)
		{
			Value = (BigInteger)value;
			Radix = 0;
			value -= decimal.Truncate(value);
			while (value != 0)
			{
				value *= 10;
				Value *= 10;
				Radix++;
				Value += (BigInteger)value;
				value -= decimal.Truncate(value);
			}
		}

		public BigFloat(BigInteger value) : this(value, 0) { }

		public BigFloat(int value) : this(value, 0) { }

		public BigFloat(uint value) : this(value, 0) { }

		public BigFloat(long value) : this(value, 0) { }

		public BigFloat(ulong value) : this(value, 0) { }

		public bool IsZero { get { return Radix >= 0 && Value.IsZero; } }

		public bool IsPositiveInfinity { get { return Radix == posinf; } }

		public bool IsNegativeInfinity { get { return Radix == neginf; } }

		public bool IsInfinity { get { return IsPositiveInfinity || IsNegativeInfinity; } }

		public bool IsNaN { get { return Radix == nan; } }

		public int Sign
		{
			get
			{
				switch (Radix)
				{
					case nan:
						return 0;
					case neginf:
						return -1;
					case posinf:
						return 1;
					default:
						return Value.Sign;
				}
			}
		}

		public BigFloat Reciprocal { get { return new BigFloat(1) / this; } }

		public static BigFloat Round(BigFloat val)
		{
			if (val.Radix <= 0)
				return val;
			BigInteger s = val.Value / BigInteger.Pow(10, val.Radix - 1);
			if (s > 0)
			{
				if (s % 10 >= 5)
					return s / 10 + 1;
			}
			else if (s % 10 <= -5)
				return s / 10 - 1;
			return s / 10;
		}

		public static BigFloat Round(BigFloat val, int radix)
		{
			if (val.Radix <= 0)
				return val;
			if (val.Radix > radix) {
				BigInteger s = val.Value / BigInteger.Pow(10, val.Radix - radix - 1);
				if (s > 0) {
					if(s % 10 >= 5)
						return new BigFloat(s / 10 + 1, radix);
				}
				else if (s % 10 <= -5)
					return new BigFloat(s / 10 - 1, radix);
				return new BigFloat(s / 10, radix);
			}
			return val;
		}

		public static BigFloat Truncate(BigFloat val)
		{
			if (val.Radix <= 0)
				return val;
			return (BigInteger)val;
		}

		public static BigFloat Truncate(BigFloat val, int radix)
		{
			if (val.Radix <= 0)
				return val;
			if(val.Radix >= radix) {
				val.Radix -= radix;
				return new BigFloat((BigInteger)val, radix);
			}
			return val;
		}

		public static BigFloat Floor(BigFloat val)
		{
			if (val.Radix <= 0)
				return val;
			BigInteger s = val.Value / BigInteger.Pow(10, val.Radix - 1);
			if (s < 0 && s % 10 < 0)
				return s / 10 - 1;
			return s / 10;
		}

		public static BigFloat Floor(BigFloat val, int radix)
		{
			if (val.Radix <= 0)
				return val;
			if (val.Radix > radix) {
				BigInteger s = val.Value / BigInteger.Pow(10, val.Radix - radix - 1);
				if (s < 0 && s % 10 < 0)
					return new BigFloat(s / 10 - 1, radix);
				return new BigFloat(s / 10, radix);
			}
			return val;
		}

		public static BigFloat Ceiling(BigFloat val)
		{
			if (val.Radix <= 0)
				return val;
			BigInteger s = val.Value / BigInteger.Pow(10, val.Radix - 1);
			if (s > 0 && s % 10 > 0)
				return s / 10 + 1;
			return s / 10;
		}

		public static BigFloat Ceiling(BigFloat val, int radix)
		{
			if (val.Radix <= 0)
				return val;
			if (val.Radix > radix) {
				BigInteger s = val.Value / BigInteger.Pow(10, val.Radix - radix - 1);
				if (s > 0 && s % 10 > 0)
					return new BigFloat(s / 10 + 1, radix);
				return new BigFloat(s / 10, radix);
			}
			return val;
		}

		public static BigFloat Exp(BigFloat val)
		{
			if (val.IsNegativeInfinity) return Zero;
			if (val.Radix<0) return val;

			BigFloat last = 0, iter = 1;
			BigInteger n = 1, fact=1;
			BigFloat sq = val;
			while (Round(iter, expmax) != Round(last, expmax)) {
				last = iter;
				iter += sq / (fact*=n);
				sq *= val;
				n++;
			}
			return Round(iter, expmax);
		}

		private static BigFloat PowBySquaring(BigFloat x, BigInteger n)
		{
			if (n == 0) {
				return 1;
			}
			if (n < 0) {
				x = x.Reciprocal;
				n = -n;
			}
			BigFloat y = 1;
			while(n > 1) {
				if(n.IsEven) {
					x *= x;
					n /= 2;
				}else {
					y *= x;
					x *= x;
					n = (n - 1) / 2;
				}
			}
			return x * y;
		}

		public static BigFloat Pow(BigFloat x, BigFloat y)
		{
			if (x.IsNaN || y.IsNaN) return NaN;
			if (y == 0) return 1;
			// TODO: handle all other special cases in here
			if (x.Radix < 0 || y.Radix < 0) return Math.Pow((double)x, (double)y);

			if (y.Radix == 0) {
				if (x.Radix == 0 && y >= 0 && y <= int.MaxValue) {
					return BigInteger.Pow(x.Value, (int)y.Value);
				}
				else {
					return PowBySquaring(x, y.Value);
				}
			}
			else {
				return Exp(y * Log(x));
			}
		}

		public static BigFloat Sin(BigFloat val)
		{
			return Math.Sin((double)val);
		}

		public static BigFloat Cos(BigFloat val)
		{
			return Math.Cos((double)val);
		}

		public static BigFloat Tan(BigFloat val)
		{
			return Math.Tan((double)val);
		}

		public static BigFloat Sinh(BigFloat val)
		{
			return Math.Sinh((double)val);
		}

		public static BigFloat Cosh(BigFloat val)
		{
			return Math.Cosh((double)val);
		}

		public static BigFloat Tanh(BigFloat val)
		{
			return Math.Tanh((double)val);
		}

		public static BigFloat Asin(BigFloat val)
		{
			return Math.Asin((double)val);
		}

		public static BigFloat Acos(BigFloat val)
		{
			return Math.Acos((double)val);
		}

		public static BigFloat Atan(BigFloat val)
		{
			return Math.Atan((double)val);
		}

		public static BigFloat Atan2(BigFloat y, BigFloat x)
		{
			return Atan(y / x);
		}

		public static BigFloat Sqrt(BigFloat val)
		{
			if (val.IsZero || val.IsPositiveInfinity || val.IsNaN)
				return val;
			if (val.Sign == -1) return BigFloat.NaN;

			BigFloat root = val / 2;
			BigFloat oroot = val / root;
			while (Round(root, sqrtmax) != Round(oroot, sqrtmax)) {
				root = (root + oroot) / 2;
				oroot = val / root;
			}
			return Round(root, sqrtmax);
		}

		public static BigFloat Abs(BigFloat val)
		{
			switch (val.Radix)
			{
				case nan:
					return NaN;
				case neginf:
				case posinf:
					return PositiveInfinity;
				default:
					return new BigFloat(BigInteger.Abs(val.Value), val.Radix);
			}
		}

		public static BigFloat Log(BigFloat val)
		{
			if (val.Sign != 1) return NaN;
			if (val.Radix < 0) return val;

			bool neg;
			if (val < 1) {
				// log(1-x)
				neg = true;
				val = 1 - val;
			} else if(val<2){
				// log(1+x)
				neg = false;
				val -= 1;
			} else if(val<4){
				return Log(Sqrt(val)) * 2;
			} else if (val <= 10) {
				return Log(Sqrt(Sqrt(val))) * 4;
			} else {
				int deltaradix = 0;
				while (val > 10) {
					val.Radix++;
					deltaradix++;
				}
				return Log(val) + deltaradix * Ln10;
			}
			BigFloat last = 1, iter = 0;
			BigInteger n = 1;
			BigFloat sq = val;
			while (Round(iter, logmax) != Round(last, logmax)) {
				last = iter;
				if (n.IsEven || neg) {
					iter -= sq / n;
				}else {
					iter += sq / n;
				}
				sq *= val;
				n++;
			}
			return Round(iter, logmax);
		}

		public static BigFloat Log(BigFloat val, BigFloat newBase)
		{
			return Log(val) / Log(newBase);
		}

		public static BigFloat Log10(BigFloat val)
		{
			return Log(val) / Ln10;
		}

		public static BigFloat operator +(BigFloat val)
		{
			return val;
		}

		public static BigFloat operator -(BigFloat val)
		{
			switch (val.Radix)
			{
				case nan:
					return NaN;
				case posinf:
					return NegativeInfinity;
				case neginf:
					return PositiveInfinity;
				default:
					return new BigFloat(-val.Value, val.Radix);
			}
		}

		public static BigFloat operator ~(BigFloat val)
		{
			if (val.Radix < 0)
				return val;
			return new BigFloat(~val.Value, val.Radix);
		}

		public static BigFloat operator ++(BigFloat val)
		{
			return val + 1;
		}

		public static BigFloat operator --(BigFloat val)
		{
			return val - 1;
		}

		public static BigFloat operator +(BigFloat a, BigFloat b)
		{
			if (a.IsNaN || b.IsNaN)
				return NaN;
			if ((a.IsPositiveInfinity && b.IsNegativeInfinity) || (a.IsNegativeInfinity && b.IsPositiveInfinity))
				return NaN;
			if (a.IsPositiveInfinity || b.IsPositiveInfinity)
				return PositiveInfinity;
			if (a.IsNegativeInfinity || b.IsNegativeInfinity)
				return NegativeInfinity;
			BigInteger valA = a.Value;
			BigInteger valB = b.Value;
			int radix;
			if (a.Radix == b.Radix)
				radix = a.Radix;
			else if (a.Radix > b.Radix)
			{
				radix = a.Radix;
				valB *= BigInteger.Pow(10, a.Radix - b.Radix);
			}
			else
			{
				radix = b.Radix;
				valA *= BigInteger.Pow(10, b.Radix - a.Radix);
			}
			return new BigFloat(valA + valB, radix);
		}

		public static BigFloat operator -(BigFloat a, BigFloat b) { return a + -b; }

		public static BigFloat operator *(BigFloat a, BigFloat b)
		{
			if (a.IsNaN || b.IsNaN)
				return NaN;
			if (a.IsInfinity || b.IsInfinity)
				switch (a.Sign * b.Sign)
				{
					case 1:
						return PositiveInfinity;
					case -1:
						return NegativeInfinity;
					case 0:
						return NaN;
				}
			return new BigFloat(a.Value * b.Value, a.Radix + b.Radix);
		}

		public static BigFloat operator /(BigFloat a, BigFloat b)
		{
			if (a.IsNaN || b.IsNaN)
				return NaN;
			if (a.IsInfinity && b.IsInfinity)
				return NaN;
			if (b.IsZero)
				switch (a.Sign)
				{
					case 1:
						return PositiveInfinity;
					case -1:
						return NegativeInfinity;
					case 0:
						return NaN;
				}
			if (a.IsInfinity)
				switch (a.Sign * b.Sign)
				{
					case 1:
						return PositiveInfinity;
					case -1:
						return NegativeInfinity;
				}
			if (b.IsInfinity)
				return Zero;
			BigInteger valA = a.Value;
			BigInteger valB = b.Value;
			if (a.Radix > b.Radix)
			{
				valB *= BigInteger.Pow(10, a.Radix - b.Radix);
			}
			else
			{
				valA *= BigInteger.Pow(10, b.Radix - a.Radix);
			}
			BigInteger result = BigInteger.Zero;
			int radix = 0;
			while (true)
			{
				result += BigInteger.DivRem(valA, valB, out valA);
				if (valA.IsZero)
					break;
				if (radix > divmax)
					break;
				radix++;
				result *= 10;
				valA *= 10;
			}
			return new BigFloat(result, radix);
		}

		public static BigFloat operator %(BigFloat a, BigFloat b)
		{
			if (a.IsNaN || b.IsNaN || a.IsInfinity || b.IsZero)
				return NaN;
			if (b.IsInfinity)
				return a;
			BigInteger valA = a.Value;
			BigInteger valB = b.Value;
			int radix;
			if (a.Radix == b.Radix)
				radix = a.Radix;
			else if (a.Radix > b.Radix)
			{
				radix = a.Radix;
				valB *= BigInteger.Pow(10, a.Radix - b.Radix);
			}
			else
			{
				radix = b.Radix;
				valA *= BigInteger.Pow(10, b.Radix - a.Radix);
			}
			return new BigFloat(valA % valB, radix);
		}

		public static BigFloat operator &(BigFloat a, BigFloat b)
		{
			if (a.Radix < 0 || b.Radix < 0)
				return NaN;
			BigInteger valA = a.Value;
			BigInteger valB = b.Value;
			int radix;
			if (a.Radix == b.Radix)
				radix = a.Radix;
			else if (a.Radix > b.Radix)
			{
				radix = a.Radix;
				valB *= BigInteger.Pow(10, a.Radix - b.Radix);
			}
			else
			{
				radix = b.Radix;
				valA *= BigInteger.Pow(10, b.Radix - a.Radix);
			}
			return new BigFloat(valA & valB, radix);
		}

		public static BigFloat operator |(BigFloat a, BigFloat b)
		{
			if (a.Radix < 0 || b.Radix < 0)
				return NaN;
			BigInteger valA = a.Value;
			BigInteger valB = b.Value;
			int radix;
			if (a.Radix == b.Radix)
				radix = a.Radix;
			else if (a.Radix > b.Radix)
			{
				radix = a.Radix;
				valB *= BigInteger.Pow(10, a.Radix - b.Radix);
			}
			else
			{
				radix = b.Radix;
				valA *= BigInteger.Pow(10, b.Radix - a.Radix);
			}
			return new BigFloat(valA | valB, radix);
		}

		public static BigFloat operator ^(BigFloat a, BigFloat b)
		{
			if (a.Radix < 0 || b.Radix < 0)
				return NaN;
			BigInteger valA = a.Value;
			BigInteger valB = b.Value;
			int radix;
			if (a.Radix == b.Radix)
				radix = a.Radix;
			else if (a.Radix > b.Radix)
			{
				radix = a.Radix;
				valB *= BigInteger.Pow(10, a.Radix - b.Radix);
			}
			else
			{
				radix = b.Radix;
				valA *= BigInteger.Pow(10, b.Radix - a.Radix);
			}
			return new BigFloat(valA ^ valB, radix);
		}

		public static bool operator ==(BigFloat a, BigFloat b)
		{
			if (a.IsNaN || b.IsNaN)
				return false;
			if (a.Radix < 0 || b.Radix < 0)
				return a.Radix == b.Radix;
			return a.Radix == b.Radix && a.Value == b.Value;
		}

		public static bool operator !=(BigFloat a, BigFloat b)
		{
			if (a.IsNaN || b.IsNaN)
				return true;
			if (a.Radix < 0 || b.Radix < 0)
				return a.Radix != b.Radix;
			return a.Radix != b.Radix || a.Value != b.Value;
		}

		public static bool operator <(BigFloat a, BigFloat b)
		{
			if (a.IsNaN || b.IsNaN)
				return false;
			if (a.IsPositiveInfinity)
				return !b.IsPositiveInfinity;
			if (a.IsNegativeInfinity)
				return false;
			BigInteger valA = a.Value;
			BigInteger valB = b.Value;
			if (a.Radix > b.Radix)
				valB *= BigInteger.Pow(10, a.Radix - b.Radix);
			else if (a.Radix < b.Radix)
				valA *= BigInteger.Pow(10, b.Radix - a.Radix);
			return valA < valB;
		}

		public static bool operator >(BigFloat a, BigFloat b)
		{
			if (a.IsNaN || b.IsNaN)
				return false;
			if (a.IsPositiveInfinity)
				return false;
			if (a.IsNegativeInfinity)
				return !b.IsNegativeInfinity;
			BigInteger valA = a.Value;
			BigInteger valB = b.Value;
			if (a.Radix > b.Radix)
				valB *= BigInteger.Pow(10, a.Radix - b.Radix);
			else if (a.Radix < b.Radix)
				valA *= BigInteger.Pow(10, b.Radix - a.Radix);
			return valA > valB;
		}

		public static bool operator <=(BigFloat a, BigFloat b)
		{
			return a == b || a < b;
		}

		public static bool operator >=(BigFloat a, BigFloat b)
		{
			return a == b || a > b;
		}

		public static implicit operator BigFloat(byte value) { return new BigFloat(value); }

		public static implicit operator BigFloat(sbyte value) { return new BigFloat(value); }

		public static implicit operator BigFloat(short value) { return new BigFloat(value); }

		public static implicit operator BigFloat(ushort value) { return new BigFloat(value); }

		public static implicit operator BigFloat(int value) { return new BigFloat(value); }

		public static implicit operator BigFloat(uint value) { return new BigFloat(value); }

		public static implicit operator BigFloat(long value) { return new BigFloat(value); }

		public static implicit operator BigFloat(ulong value) { return new BigFloat(value); }

		public static implicit operator BigFloat(float value) { return new BigFloat(value); }

		public static implicit operator BigFloat(double value) { return new BigFloat(value); }

		public static implicit operator BigFloat(decimal value) { return new BigFloat(value); }

		public static implicit operator BigFloat(BigInteger value) { return new BigFloat(value); }

		public static explicit operator byte(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return 0;
				case posinf:
					return byte.MaxValue;
				case neginf:
					return byte.MinValue;
				default:
					return (byte)(value.Value / BigInteger.Pow(10, value.Radix));
			}
		}

		public static explicit operator sbyte(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return 0;
				case posinf:
					return sbyte.MaxValue;
				case neginf:
					return sbyte.MinValue;
				default:
					return (sbyte)(value.Value / BigInteger.Pow(10, value.Radix));
			}
		}

		public static explicit operator short(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return 0;
				case posinf:
					return short.MaxValue;
				case neginf:
					return short.MinValue;
				default:
					return (short)(value.Value / BigInteger.Pow(10, value.Radix));
			}
		}

		public static explicit operator ushort(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return 0;
				case posinf:
					return ushort.MaxValue;
				case neginf:
					return ushort.MinValue;
				default:
					return (ushort)(value.Value / BigInteger.Pow(10, value.Radix));
			}
		}

		public static explicit operator int(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return 0;
				case posinf:
					return int.MaxValue;
				case neginf:
					return int.MinValue;
				default:
					return (int)(value.Value / BigInteger.Pow(10, value.Radix));
			}
		}

		public static explicit operator uint(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return 0;
				case posinf:
					return uint.MaxValue;
				case neginf:
					return uint.MinValue;
				default:
					return (uint)(value.Value / BigInteger.Pow(10, value.Radix));
			}
		}

		public static explicit operator long(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return 0;
				case posinf:
					return long.MaxValue;
				case neginf:
					return long.MinValue;
				default:
					return (long)(value.Value / BigInteger.Pow(10, value.Radix));
			}
		}

		public static explicit operator ulong(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return 0;
				case posinf:
					return ulong.MaxValue;
				case neginf:
					return ulong.MinValue;
				default:
					return (ulong)(value.Value / BigInteger.Pow(10, value.Radix));
			}
		}

		public static explicit operator float(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return float.NaN;
				case posinf:
					return float.PositiveInfinity;
				case neginf:
					return float.NegativeInfinity;
				default:
					BigInteger rem;
					BigInteger res = BigInteger.DivRem(value.Value, BigInteger.Pow(10, value.Radix), out rem);
					return (float)res + (float)rem / (float)Math.Pow(10, value.Radix);
			}
		}

		public static explicit operator double(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return double.NaN;
				case posinf:
					return double.PositiveInfinity;
				case neginf:
					return double.NegativeInfinity;
				default:
					BigInteger rem;
					BigInteger res = BigInteger.DivRem(value.Value, BigInteger.Pow(10, value.Radix), out rem);
					return (double)res + (double)rem / Math.Pow(10, value.Radix);
			}
		}

		public static explicit operator decimal(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return decimal.Zero;
				case posinf:
					return decimal.MaxValue;
				case neginf:
					return decimal.MinValue;
				default:
					BigInteger rem;
					BigInteger res = BigInteger.DivRem(value.Value, BigInteger.Pow(10, value.Radix), out rem);
					return (decimal)res + (decimal)rem / (decimal)Math.Pow(10, value.Radix);
			}
		}

		public static explicit operator BigInteger(BigFloat value)
		{
			switch (value.Radix)
			{
				case nan:
					return BigInteger.Zero;
				case posinf:
					return BigInteger.One;
				case neginf:
					return BigInteger.MinusOne;
				default:
					return value.Value / BigInteger.Pow(10, value.Radix);
			}
		}

		public static readonly BigFloat Zero = new BigFloat();

		public static readonly BigFloat PositiveInfinity = new BigFloat(0, posinf);

		public static readonly BigFloat NegativeInfinity = new BigFloat(0, neginf);

		public static readonly BigFloat NaN = new BigFloat(0, nan);

		public static readonly BigFloat E = Exp(1);

		public static readonly BigFloat PI = new BigFloat(0x39, 0x00, 0x00, 0x00, 0x2E, 0x09, 0xCF, 0x68, 0x28, 0x91, 0xE5, 0x32, 0xDF, 0x62, 0x62, 0x37, 0xD3, 0x70, 0xBD, 0x22, 0x05, 0x23, 0x30, 0x5E, 0xFA, 0xC1, 0x1F, 0x80, 0x00);

		// Calculated with logmax = 100
		public static readonly BigFloat Ln10 = new BigFloat(0x64, 0x00, 0x00, 0x00, 0x88, 0x51, 0x81, 0x0A, 0xA0, 0x03, 0xDD, 0x3D, 0xF0, 0xAD, 0x42, 0x3C, 0x70, 0xDD, 0x55, 0xFC, 0x52, 0xFB, 0xEB, 0xA3, 0x3A, 0x04, 0x25, 0x34, 0x2A, 0x19, 0x13, 0x65, 0x02, 0x2A, 0x8C, 0xA5, 0xD8, 0xA8, 0x00, 0xC7, 0xF1, 0x94, 0x4B, 0xF5, 0x1B, 0x2A);

		public override string ToString()
		{
			switch (Radix)
			{
				case nan:
					return "NaN";
				case posinf:
					return "Infinity";
				case neginf:
					return "-Infinity";
				case 0:
					return Value.ToString("R");
				default:
					bool m = this < Zero;
					string str = BigInteger.Abs(Value).ToString("R").PadLeft(Radix + 1, '0');
					str = str.Insert(str.Length - Radix, ".");
					if (m)
						return "-" + str;
					return str;
			}
		}

		public static BigFloat Parse(string s)
		{
			if (s.Equals("nan", StringComparison.OrdinalIgnoreCase))
				return NaN;
			if (s.Equals("infinity", StringComparison.OrdinalIgnoreCase))
				return PositiveInfinity;
			if (s.Equals("-infiniy", StringComparison.OrdinalIgnoreCase))
				return NegativeInfinity;
			s = ProcessScientificString(s);
			if (!s.Contains("."))
				return BigInteger.Parse(s);
			string[] split = s.Split('.');
			return new BigFloat(BigInteger.Parse(split[0] + split[1]), split[1].Length);
		}

		public byte[] ToByteArray()
		{
			byte[] d2 = Value.ToByteArray();
			byte[] data = new byte[d2.Length + sizeof(int)];
			BitConverter.GetBytes(Radix).CopyTo(data, 0);
			d2.CopyTo(data, sizeof(int));
			return data;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is BigFloat)) return false;
			return this == (BigFloat)obj;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode() ^ Radix.GetHashCode();
		}

		private static string ProcessScientificString(string str)
		{
			if (!str.Contains("E") & !str.Contains("e"))
				return str;
			str = str.ToUpper();
			char decSeparator = '.';
			string[] exponentParts = str.Split('E');
			string[] decimalParts = exponentParts[0].Split(decSeparator);
			if (decimalParts.Length == 1)
				decimalParts = new string[] {
				exponentParts[0],
				"0"
			};
			int exponentValue = int.Parse(exponentParts[1]);
			string newNumber = decimalParts[0] + decimalParts[1];
			string result = null;
			if (exponentValue > 0)
				result = newNumber + GetZeros(exponentValue - decimalParts[1].Length);
			else
			{
				result = string.Empty;
				if (newNumber.StartsWith("-"))
				{
					result = "-";
					newNumber = newNumber.Substring(1);
				}
				result += "0" + decSeparator + GetZeros(exponentValue + decimalParts[0].Length) + newNumber;
				result = result.TrimEnd('0');
			}
			return result;
		}

		private static string ToLongString(double input)
		{
			return ProcessScientificString(input.ToString("R", System.Globalization.NumberFormatInfo.InvariantInfo));
		}

		private static string ToLongString(float input)
		{
			return ProcessScientificString(input.ToString("R", System.Globalization.NumberFormatInfo.InvariantInfo));
		}

		private static string GetZeros(int zeroCount)
		{
			return new string('0', Math.Abs(zeroCount));
		}
	}
}