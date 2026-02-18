#include <cstdint>
#include <cstddef>
#include <immintrin.h>

#define EXPORT extern "C" __declspec(dllexport)

struct RawGameStringData
{
    uint64_t Id;
    uint64_t ParentId;
};

/* ============================================================
   AVX2 16-Digit Parser
   ============================================================ */

static inline uint64_t ParseUlong16(const uint8_t* p)
{
    __m128i bytes = _mm_loadu_si128((const __m128i*)p);
    bytes = _mm_sub_epi8(bytes, _mm_set1_epi8('0'));

    const __m128i mul10 = _mm_setr_epi8(
        10,1, 10,1, 10,1, 10,1,
        10,1, 10,1, 10,1, 10,1);

    __m128i pairs = _mm_maddubs_epi16(bytes, mul10);

    const __m128i mul100 = _mm_setr_epi16(
        100,1, 100,1, 100,1, 100,1);

    __m128i quads = _mm_madd_epi16(pairs, mul100);

    alignas(16) uint32_t tmp[4];
    _mm_store_si128((__m128i*)tmp, quads);

    uint64_t hi = (uint64_t)tmp[0] * 10000ULL + tmp[1];
    uint64_t lo = (uint64_t)tmp[2] * 10000ULL + tmp[3];

    return hi * 100000000ULL + lo;
}

/* ============================================================
   15-Digit Variant (Corrected)
   ============================================================ */

static inline uint32_t ParseUint8_AVX2(const uint8_t* p)
{
    __m128i bytes = _mm_loadl_epi64((const __m128i*)p);
    bytes = _mm_sub_epi8(bytes, _mm_set1_epi8('0'));

    const __m128i mul10 = _mm_setr_epi8(
        10,1, 10,1, 10,1, 10,1,
        0,0, 0,0, 0,0, 0,0);

    __m128i pairs = _mm_maddubs_epi16(bytes, mul10);

    const __m128i mul100 = _mm_setr_epi16(
        100,1, 100,1,
        0,0, 0,0);

    __m128i quads = _mm_madd_epi16(pairs, mul100);

    alignas(16) uint32_t tmp[4];
    _mm_store_si128((__m128i*)tmp, quads);

    return tmp[0] * 10000u + tmp[1];
}


static inline uint64_t ParseUlong15(const uint8_t* p)
{
    uint64_t hi =
        (uint64_t)(p[0] - '0') * 1000000ULL +
        (uint64_t)(p[1] - '0') * 100000ULL +
        (uint64_t)(p[2] - '0') * 10000ULL +
        (uint64_t)(p[3] - '0') * 1000ULL +
        (uint64_t)(p[4] - '0') * 100ULL +
        (uint64_t)(p[5] - '0') * 10ULL +
        (uint64_t)(p[6] - '0');

    uint64_t lo = ParseUint8_AVX2(p + 7);

    return hi * 100000000ULL + lo;
}

/* ============================================================
   17-Digit Variant
   ============================================================ */

static inline uint64_t ParseUlong17(const uint8_t* p)
{
    uint64_t x = ParseUlong16(p);
    return x * 10ULL + (uint64_t)(p[16] - '0');
}

/* ============================================================
   Parent Selector
   ============================================================ */

static inline uint64_t ParseParent(const uint8_t* p, int& len)
{
    switch (p[0])
    {
        case '2':
            len = 15;
            return ParseUlong15(p);

        case '1':
            len = 17;
            return ParseUlong17(p);

        default:
            len = 16;
            return ParseUlong16(p);
    }
}

/* ============================================================
   Main Entry
   ============================================================ */

EXPORT uint64_t __cdecl ParseBuffer(
    const uint8_t* __restrict data,
    size_t size,
    RawGameStringData* __restrict output)
{
    if (size < 116)
        return 0;

    const uint8_t* p = data + 106;
    const uint8_t* end = data + size - 10;

    RawGameStringData* write = output;

    while (p + 78 < end)
    {
        p += 12;

        const uint64_t id = ParseUlong16(p);
        p += 16;

        p += 16;

        int digits = 0;
        const uint64_t parent = ParseParent(p, digits);

        p += digits + 22;

        write->Id = id;
        write->ParentId = parent;
        write++;
    }

    return (uint64_t)(write - output);
}
