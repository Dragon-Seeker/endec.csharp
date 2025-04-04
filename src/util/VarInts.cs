using System;

namespace io.wispforest.util;

public class VarInts {
    public const int SEGMENT_BITS = 127;
    public const int CONTINUE_BIT = 128;

    public static int getSizeInBytesFromInt(int i){
        for(int j = 1; j < 5; ++j) {
            if ((i & -1 << j * 7) == 0) {
                return j;
            }
        }

        return 5;
    }

    public static int getSizeInBytesFromLong(long l) {
        for(int i = 1; i < 10; ++i) {
            if ((l & -1L << i * 7) == 0L) {
                return i;
            }
        }

        return 10;
    }

    public static int readInt(Func<byte> readByteSup) {
        int value = 0;
        int position = 0;
        byte currentByte;

        while (true) {
            currentByte = readByteSup();
            value |= (currentByte & SEGMENT_BITS) << position;

            if ((currentByte & CONTINUE_BIT) == 0) break;

            position += 7;

            if (position >= 32) throw new Exception("VarInt is too big");
        }

        return value;
    }

    public static void writeInt(int value, Action<byte> writeByteFunc) {
        while (true) {
            if ((value & ~SEGMENT_BITS) == 0) {
                writeByteFunc((byte) value);
                return;
            }

            writeByteFunc((byte) ((value & SEGMENT_BITS) | CONTINUE_BIT));

            // Note: >>> means that the sign bit is shifted with the rest of the number rather than being left alone
            value >>>= 7;
        }
    }

    public static long readLong(Func<byte> readByteSup) {
        long value = 0;
        int position = 0;
        byte currentByte;

        while (true) {
            currentByte = readByteSup();
            value |= (long) (currentByte & SEGMENT_BITS) << position;

            if ((currentByte & CONTINUE_BIT) == 0) break;

            position += 7;

            if (position >= 64) throw new Exception("VarLong is too big");
        }

        return value;
    }

    public static void writeLong(long value, Action<byte> writeByteFunc) {
        while (true) {
            if ((value & ~((long)SEGMENT_BITS)) == 0) {
                writeByteFunc((byte)value);
                return;
            }

            writeByteFunc((byte)((value & SEGMENT_BITS) | CONTINUE_BIT));

            // Note: >>> means that the sign bit is shifted with the rest of the number rather than being left alone
            value >>>= 7;
        }
    }
}