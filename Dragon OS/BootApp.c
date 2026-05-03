typedef unsigned char UINT8;
typedef unsigned short UINT16;
typedef unsigned int UINT32;
typedef unsigned long long UINT64;
typedef signed int INT32;
typedef signed long long INT64;
typedef signed char INT8;
typedef UINT64 UINTN;
typedef void* EFI_HANDLE;
typedef void* EFI_EVENT;
typedef UINT64 EFI_STATUS;

#include "generated_bitmap.h"

unsigned char __cdecl __inbyte(unsigned short port);
void __cdecl __outbyte(unsigned short port, unsigned char data);
unsigned __int64 __rdtsc(void);

#pragma intrinsic(__inbyte)
#pragma intrinsic(__outbyte)
#pragma intrinsic(__rdtsc)

typedef struct
{
    UINT32 Data1;
    UINT16 Data2;
    UINT16 Data3;
    UINT8 Data4[8];
} EFI_GUID;

typedef EFI_STATUS (*EFI_LOCATE_PROTOCOL)(EFI_GUID* protocol, void* registration, void** interfaceOut);
typedef EFI_STATUS (*EFI_STALL)(UINTN microseconds);
typedef EFI_STATUS (*EFI_WAIT_FOR_EVENT)(UINTN numberOfEvents, EFI_EVENT* event, UINTN* indexOut);
typedef EFI_STATUS (*EFI_GET_MEMORY_MAP)(UINTN* memoryMapSize, void* memoryMap, UINTN* mapKey, UINTN* descriptorSize, UINT32* descriptorVersion);
typedef EFI_STATUS (*EFI_ALLOCATE_POOL)(UINT32 poolType, UINTN size, void** buffer);
typedef EFI_STATUS (*EFI_FREE_POOL)(void* buffer);
typedef EFI_STATUS (*EFI_HANDLE_PROTOCOL)(EFI_HANDLE handle, EFI_GUID* protocol, void** interfaceOut);
typedef EFI_STATUS (*EFI_LOCATE_HANDLE_BUFFER)(UINT32 searchType, EFI_GUID* protocol, void* searchKey, UINTN* noHandles, EFI_HANDLE** buffer);
typedef EFI_STATUS (*EFI_EXIT_BOOT_SERVICES)(EFI_HANDLE imageHandle, UINTN mapKey);
typedef EFI_STATUS (*EFI_INPUT_RESET)(void* thisProtocol, UINT8 extendedVerification);
typedef EFI_STATUS (*EFI_INPUT_READ_KEY)(void* thisProtocol, void* keyOut);
typedef EFI_STATUS (*EFI_SIMPLE_POINTER_RESET)(void* thisProtocol, UINT8 extendedVerification);
typedef EFI_STATUS (*EFI_SIMPLE_POINTER_GET_STATE)(void* thisProtocol, void* stateOut);
typedef EFI_STATUS (*EFI_ABSOLUTE_POINTER_RESET)(void* thisProtocol, UINT8 extendedVerification);
typedef EFI_STATUS (*EFI_ABSOLUTE_POINTER_GET_STATE)(void* thisProtocol, void* stateOut);
typedef EFI_STATUS (*EFI_PCI_IO_PROTOCOL_CONFIG)(void* thisProtocol, UINT32 width, UINT32 offset, UINTN count, void* buffer);
typedef EFI_STATUS (*EFI_PCI_IO_PROTOCOL_GET_LOCATION)(void* thisProtocol, UINTN* segmentNumber, UINTN* busNumber, UINTN* deviceNumber, UINTN* functionNumber);

typedef struct
{
    INT32 RelativeMovementX;
    INT32 RelativeMovementY;
    INT32 RelativeMovementZ;
    UINT8 LeftButton;
    UINT8 RightButton;
} EFI_SIMPLE_POINTER_STATE;

typedef struct
{
    UINT64 ResolutionX;
    UINT64 ResolutionY;
    UINT64 ResolutionZ;
    UINT8 LeftButton;
    UINT8 RightButton;
} EFI_SIMPLE_POINTER_MODE;

typedef struct
{
    EFI_SIMPLE_POINTER_RESET Reset;
    EFI_SIMPLE_POINTER_GET_STATE GetState;
    EFI_EVENT WaitForInput;
    EFI_SIMPLE_POINTER_MODE* Mode;
} EFI_SIMPLE_POINTER_PROTOCOL;

typedef struct
{
    UINT64 CurrentX;
    UINT64 CurrentY;
    UINT64 CurrentZ;
    UINT32 ActiveButtons;
} EFI_ABSOLUTE_POINTER_STATE;

typedef struct
{
    UINT64 AbsoluteMinX;
    UINT64 AbsoluteMinY;
    UINT64 AbsoluteMinZ;
    UINT64 AbsoluteMaxX;
    UINT64 AbsoluteMaxY;
    UINT64 AbsoluteMaxZ;
    UINT32 Attributes;
} EFI_ABSOLUTE_POINTER_MODE;

typedef struct
{
    EFI_ABSOLUTE_POINTER_RESET Reset;
    EFI_ABSOLUTE_POINTER_GET_STATE GetState;
    EFI_EVENT WaitForInput;
    EFI_ABSOLUTE_POINTER_MODE* Mode;
} EFI_ABSOLUTE_POINTER_PROTOCOL;

typedef struct
{
    UINT32 ScreenWidth;
    UINT32 ScreenHeight;
    UINT32 PixelsPerScanLine;
    UINT32 PixelFormat;
    UINT32 RedMask;
    UINT32 GreenMask;
    UINT32 BlueMask;
    UINT32* Framebuffer;
} FRAMEBUFFER_INFO;

typedef struct
{
    EFI_PCI_IO_PROTOCOL_CONFIG Read;
    EFI_PCI_IO_PROTOCOL_CONFIG Write;
} EFI_PCI_IO_PROTOCOL_CONFIG_ACCESS;

typedef struct
{
    void* PollMem;
    void* PollIo;
    void* Mem[2];
    void* Io[2];
    EFI_PCI_IO_PROTOCOL_CONFIG_ACCESS Pci;
    void* CopyMem;
    void* Map;
    void* Unmap;
    void* AllocateBuffer;
    void* FreeBuffer;
    void* Flush;
    EFI_PCI_IO_PROTOCOL_GET_LOCATION GetLocation;
    void* Attributes;
    void* GetBarAttributes;
    void* SetBarAttributes;
    UINT64 RomSize;
    void* RomImage;
} EFI_PCI_IO_PROTOCOL;

typedef struct
{
    UINT32 X;
    UINT32 Y;
    UINT32 SavedWidth;
    UINT32 SavedHeight;
    UINT8 Visible;
    UINT32 SavedPixels[CURSOR_WIDTH * CURSOR_HEIGHT];
    UINT32 ResolvedPixels[CURSOR_WIDTH * CURSOR_HEIGHT];
    UINT8 OpaqueMask[CURSOR_WIDTH * CURSOR_HEIGHT];
    INT32 AccumX;
    INT32 AccumY;
    UINT8 LeftButtonDown;
    INT32 LastRawX;
    INT32 LastRawY;
    INT32 LastPixelX;
    INT32 LastPixelY;
    UINT32 LastPollCount;
} SOFTWARE_CURSOR;

typedef struct
{
    UINT8 Enabled;
    UINT8 PacketIndex;
    UINT8 LeftButtonDown;
    UINT8 Reserved;
    UINT8 Packet[4];
    INT32 LastDeltaX;
    INT32 LastDeltaY;
    UINT32 PacketCount;
} PS2_MOUSE_STATE;

typedef struct
{
    UINT32 X;
    UINT32 Y;
    UINT32 Width;
    UINT32 Height;
    UINT32 Radius;
    UINT32 Color;
    UINT8 Dragging;
    INT32 DragOffsetX;
    INT32 DragOffsetY;
} UI_BOX;

typedef struct
{
    UI_BOX Panel;
    UI_BOX InstallButton;
    UI_BOX TryButton;
    UINT32 HoveredButton;
    UINT8 MouseDetected;
} SETUP_UI_STATE;

typedef struct
{
    UINT16 VendorId;
    UINT16 DeviceId;
    UINT8 ClassCode;
    UINT8 Subclass;
    UINT8 ProgIf;
    UINT8 Kind;
    UINT8 Segment;
    UINT8 Bus;
    UINT8 Device;
    UINT8 Function;
} PCI_DEVICE_INFO;

typedef struct
{
    FRAMEBUFFER_INFO Framebuffer;
    UINT32* ShadowFramebuffer;
    UINT32* ComposeFramebuffer;
    UINT64 TscTicksPerSecond;
    void* MemoryMap;
    UINTN MemoryMapSize;
    UINTN MemoryMapKey;
    UINTN MemoryDescriptorSize;
    UINT32 MemoryDescriptorVersion;
    UINTN MemoryDescriptorCount;
    UINTN GpuCount;
    UINTN AudioCount;
    UINTN PciDeviceCount;
    PCI_DEVICE_INFO PciDevices[32];
} KERNEL_BOOT_INFO;

#define EFI_SUCCESS 0u
#define EFI_ERROR_BIT 0x8000000000000000ull
#define EFI_INVALID_PARAMETER (EFI_ERROR_BIT | 2ull)
#define EFI_UNSUPPORTED (EFI_ERROR_BIT | 3ull)
#define EFI_BUFFER_TOO_SMALL (EFI_ERROR_BIT | 5ull)
#define EFI_NOT_READY (EFI_ERROR_BIT | 6ull)

#define GOP_PIXEL_RED_GREEN_BLUE_RESERVED_8BIT 0u
#define GOP_PIXEL_BLUE_GREEN_RED_RESERVED_8BIT 1u
#define GOP_PIXEL_BIT_MASK 2u
#define GOP_PIXEL_BLT_ONLY 3u

#define EFI_BY_PROTOCOL 2u
#define EFI_BOOT_SERVICES_DATA 4u
#define EFI_PCI_IO_PROTOCOL_WIDTH_UINT32 2u
#define KERNEL_PCI_KIND_GPU 1u
#define KERNEL_PCI_KIND_AUDIO 2u

#define EFI_SYSTEM_TABLE_BOOT_SERVICES_OFFSET 0x60u
#define EFI_BOOT_SERVICES_GET_MEMORY_MAP_OFFSET 0x38u
#define EFI_BOOT_SERVICES_ALLOCATE_POOL_OFFSET 0x40u
#define EFI_BOOT_SERVICES_FREE_POOL_OFFSET 0x48u
#define EFI_BOOT_SERVICES_LOCATE_PROTOCOL_OFFSET 0x140u
#define EFI_BOOT_SERVICES_HANDLE_PROTOCOL_OFFSET 0x98u
#define EFI_BOOT_SERVICES_EXIT_BOOT_SERVICES_OFFSET 0xE8u
#define EFI_BOOT_SERVICES_LOCATE_HANDLE_BUFFER_OFFSET 0x138u
#define EFI_BOOT_SERVICES_STALL_OFFSET 0xF8u
#define GOP_MODE_OFFSET 0x18u
#define GOP_MODE_INFO_OFFSET 0x08u
#define GOP_MODE_FRAMEBUFFER_BASE_OFFSET 0x18u
#define GOP_MODE_FRAMEBUFFER_SIZE_OFFSET 0x20u

#define GOP_INFO_HORIZONTAL_RESOLUTION_OFFSET 0x04u
#define GOP_INFO_VERTICAL_RESOLUTION_OFFSET 0x08u
#define GOP_INFO_PIXEL_FORMAT_OFFSET 0x0Cu
#define GOP_INFO_RED_MASK_OFFSET 0x10u
#define GOP_INFO_GREEN_MASK_OFFSET 0x14u
#define GOP_INFO_BLUE_MASK_OFFSET 0x18u
#define GOP_INFO_PIXELS_PER_SCANLINE_OFFSET 0x20u

static EFI_GUID gGraphicsOutputProtocolGuid =
{
    0x9042A9DEu,
    0x23DCu,
    0x4A38u,
    { 0x96u, 0xFBu, 0x7Au, 0xDEu, 0xD0u, 0x80u, 0x51u, 0x6Au }
};

static EFI_GUID gSimplePointerProtocolGuid =
{
    0x31878C87u,
    0x0B75u,
    0x11D5u,
    { 0x9Au, 0x4Fu, 0x00u, 0x90u, 0x27u, 0x3Fu, 0xC1u, 0x4Du }
};

static EFI_GUID gAbsolutePointerProtocolGuid =
{
    0x8D59D32Bu,
    0xC655u,
    0x4AE9u,
    { 0x9Bu, 0x15u, 0xF2u, 0x59u, 0x04u, 0x99u, 0x2Au, 0x43u }
};

static EFI_GUID gPciIoProtocolGuid =
{
    0x4CF5B200u,
    0x68B8u,
    0x4CA5u,
    { 0x9Eu, 0xECu, 0xB2u, 0x3Eu, 0x3Fu, 0x50u, 0x02u, 0x9Au }
};

static UINT32 MaskShift(UINT32 mask);
static UINT32 MaskWidth(UINT32 mask, UINT32 shift);
static UINT32 ScaleChannel(UINT32 channel, UINT32 mask);
static UINT8 ExpandChannel(UINT32 channel, UINT32 width);
static UINT32 ComposePixel(UINT8 red, UINT8 green, UINT8 blue, UINT32 pixelFormat, UINT32 redMask, UINT32 greenMask, UINT32 blueMask);
static void DecomposePixel(UINT32 pixel, UINT32 pixelFormat, UINT32 redMask, UINT32 greenMask, UINT32 blueMask, UINT8* redOut, UINT8* greenOut, UINT8* blueOut);
static UINT32 AlphaBlendPixel(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 destinationPixel, UINT8 red, UINT8 green, UINT8 blue, UINT8 alpha);
static EFI_STATUS InitializeFramebuffer(void* systemTable, FRAMEBUFFER_INFO* framebufferInfo, EFI_STALL* stallOut);
static void ClearScreen(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 color, UINT8 IgnoreBlackPixels);
static void FillPixels32(UINT32* destination, UINT32 count, UINT32 color);
static void CopyPixels32(UINT32* destination, const UINT32* source, UINT32 count);
static void BlitSurfaceRect(const FRAMEBUFFER_INFO* destination, const FRAMEBUFFER_INFO* source, UINT32 x, UINT32 y, UINT32 width, UINT32 height);
static UINT8 BlendChannelOverBlack(UINT8 channel, UINT8 alpha);
static void ComputeSplashRect(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 scale1024, UINT32* startXOut, UINT32* startYOut, UINT32* drawWidthOut, UINT32* drawHeightOut);
static void DrawSplashFrame(const FRAMEBUFFER_INFO* framebufferInfo, UINT8 alpha, UINT32 scale1024);
static void PlaySplashSequence(const FRAMEBUFFER_INFO* framebufferInfo, EFI_STALL stall);
static void InitializeCursorSurface(const FRAMEBUFFER_INFO* framebufferInfo, SOFTWARE_CURSOR* cursor);
static void RestoreCursorBackground(const FRAMEBUFFER_INFO* framebufferInfo, SOFTWARE_CURSOR* cursor);
static void SaveCursorBackground(const FRAMEBUFFER_INFO* framebufferInfo, SOFTWARE_CURSOR* cursor);
static void DrawCursor(const FRAMEBUFFER_INFO* framebufferInfo, const SOFTWARE_CURSOR* cursor);
static void ClearRect(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 x, UINT32 y, UINT32 width, UINT32 height, UINT32 color);
static void DrawRoundedBox(const FRAMEBUFFER_INFO* framebufferInfo, const UI_BOX* box);
static UINT8 PointInRoundedBox(UINT32 x, UINT32 y, const UI_BOX* box);
static void DrawGlyph(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT8 glyph, UINT8 red, UINT8 green, UINT8 blue);
static void DrawTextString(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 red, UINT8 green, UINT8 blue);
static void DrawGlyphScaled(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT8 glyph, UINT8 red, UINT8 green, UINT8 blue, UINT32 scale);
static void DrawTextStringScaled(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 red, UINT8 green, UINT8 blue, UINT32 scale);
static INT32 MeasureTextStringScaled(const char* text, UINT32 scale);
static void DrawGradientOutlinedTextScaled(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT32 scale, UINT8 startRed, UINT8 startGreen, UINT8 startBlue, UINT8 endRed, UINT8 endGreen, UINT8 endBlue, UINT32 outlineThickness);
static void DrawGlyphAtlas(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT8 glyph, UINT8 red, UINT8 green, UINT8 blue, UINT32 firstChar, UINT32 glyphCount, UINT32 glyphWidth, UINT32 glyphHeight, const UINT8* glyphAlpha);
static void DrawTextStringAtlas(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 red, UINT8 green, UINT8 blue, UINT32 firstChar, UINT32 glyphCount, UINT32 glyphWidth, UINT32 glyphHeight, const UINT8* glyphAlpha, const UINT8* glyphAdvance);
static INT32 MeasureTextStringAtlas(const char* text, UINT32 firstChar, UINT32 glyphCount, const UINT8* glyphAdvance, UINT32 fallbackAdvance);
static void DrawGradientOutlinedTextAtlas(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 startRed, UINT8 startGreen, UINT8 startBlue, UINT8 endRed, UINT8 endGreen, UINT8 endBlue, UINT32 outlineThickness, UINT32 firstChar, UINT32 glyphCount, UINT32 glyphWidth, UINT32 glyphHeight, const UINT8* glyphAlpha, const UINT8* glyphAdvance, UINT32 fallbackAdvance);
static void DrawIconGlyph(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT8 glyph, UINT8 red, UINT8 green, UINT8 blue);
static void DrawIconTextString(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 red, UINT8 green, UINT8 blue);
static void DrawImageNearestAlpha(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT32 drawWidth, UINT32 drawHeight, UINT32 sourceWidth, UINT32 sourceHeight, const UINT8* pixels, const UINT8* alpha);
static void BlitCursorComposite(const FRAMEBUFFER_INFO* destinationFramebufferInfo, const FRAMEBUFFER_INFO* shadowFramebufferInfo, FRAMEBUFFER_INFO* composeFramebufferInfo, SOFTWARE_CURSOR* cursor);
static UINT16 EvaluateTween(UINT32 tweenFamily, UINT32 tweenMode, UINT32 progress1024);
static void AppendText(char** cursor, const char* text);
static void AppendInt(char** cursor, INT32 value);
static void AppendUIntHexFixed(char** cursor, UINT32 value, UINT32 digits);
static UINT32 CountTextLines(const char* text);
static UINT32 CountIconTextLines(const char* text);
static INT32 AdvanceConsoleBlockY(INT32 y, const char* text);
static INT32 AdvanceIconConsoleBlockY(INT32 y, const char* text);
static INT32 DrawConsoleBlock(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text);
static INT32 DrawIconConsoleBlock(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text);
static void DrawMouseDebug(const FRAMEBUFFER_INFO* framebufferInfo, const SOFTWARE_CURSOR* cursor);
static UINT32 ClampCoordinate(INT32 value, UINT32 limit);
static UINT32 ClampCursorX(INT32 value, const FRAMEBUFFER_INFO* framebufferInfo);
static UINT32 ClampCursorY(INT32 value, const FRAMEBUFFER_INFO* framebufferInfo);
static INT32 CountsToFixedPixels(INT32 counts, UINT64 resolution, UINT32 screenSpan);
static INT32 ApplyMouseAcceleration(INT32 deltaFixed);
static INT32 ClampI32(INT32 value, INT32 minValue, INT32 maxValue);
static UINT8 TryUpdateCursorFromSimplePointer(EFI_SIMPLE_POINTER_PROTOCOL* pointer, SOFTWARE_CURSOR* cursor, const FRAMEBUFFER_INFO* framebufferInfo);
static UINT8 TryUpdateCursorFromAbsolutePointer(EFI_ABSOLUTE_POINTER_PROTOCOL* pointer, SOFTWARE_CURSOR* cursor, const FRAMEBUFFER_INFO* framebufferInfo);
static void BusyPause(UINT32 iterations);
static UINT8 Ps2WaitForInputClear(void);
static UINT8 Ps2WaitForOutputByte(UINT8 requireMouseData, UINT8* dataOut);
static UINT8 Ps2TryReadByte(UINT8* dataOut, UINT8* isMouseDataOut);
static void Ps2FlushOutputBuffer(void);
static UINT8 Ps2WriteControllerCommand(UINT8 command);
static UINT8 Ps2WriteControllerData(UINT8 value);
static UINT8 Ps2WriteMouseCommand(UINT8 command);
static UINT8 InitializePs2Mouse(PS2_MOUSE_STATE* mouseState);
static UINT8 UpdateCursorFromPs2Mouse(PS2_MOUSE_STATE* mouseState, SOFTWARE_CURSOR* cursor, const FRAMEBUFFER_INFO* framebufferInfo);
static void InitializeSetupUiState(const FRAMEBUFFER_INFO* framebufferInfo, SETUP_UI_STATE* setupUiState, UINT8 mouseDetected);
static UINT32 HitTestSetupUi(const SETUP_UI_STATE* setupUiState, const SOFTWARE_CURSOR* cursor);
static void DrawSetupButton(const FRAMEBUFFER_INFO* framebufferInfo, const UI_BOX* button, const char* label, UINT8 hovered);
static void RefreshSetupButtons(const FRAMEBUFFER_INFO* framebufferInfo, const SETUP_UI_STATE* setupUiState, UINT32 previousHoveredButton, UINT32 currentHoveredButton);
static void DrawSetupScreen(const FRAMEBUFFER_INFO* framebufferInfo, const SETUP_UI_STATE* setupUiState);
static void RunKernelMouseLoop(const KERNEL_BOOT_INFO* bootInfo);
static void DrawTweenDemoBase(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 tweenFamily);
static void DrawTweenDemoMotion(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 tweenFamily, UINT32 progress1024);
static void GetTweenDemoDynamicRect(const FRAMEBUFFER_INFO* framebufferInfo, UINT32* xOut, UINT32* yOut, UINT32* widthOut, UINT32* heightOut);
static void DrawTweenDemoFrame(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 tweenFamily, UINT32 progress1024);
static UINT64 EstimateTscTicksPerSecond(EFI_STALL stall);
static void PreserveUiSubsystemReferences(void);
static void CollectPciDevices(EFI_LOCATE_HANDLE_BUFFER locateHandleBuffer, EFI_HANDLE_PROTOCOL handleProtocol, EFI_FREE_POOL freePool, KERNEL_BOOT_INFO* bootInfo);
static EFI_STATUS ExitBootServicesForKernel(EFI_HANDLE imageHandle, EFI_GET_MEMORY_MAP getMemoryMap, EFI_ALLOCATE_POOL allocatePool, EFI_EXIT_BOOT_SERVICES exitBootServices, KERNEL_BOOT_INFO* bootInfo);
static void DrawKernelStatusScreen(const KERNEL_BOOT_INFO* bootInfo);

static UINT32 MaskShift(UINT32 mask)
{
    UINT32 shift = 0u;
    while (((mask >> shift) & 1u) == 0u)
    {
        shift++;
    }

    return shift;
}

static UINT32 MaskWidth(UINT32 mask, UINT32 shift)
{
    UINT32 width = 0u;
    UINT32 value = mask >> shift;
    while ((value & 1u) != 0u)
    {
        width++;
        value >>= 1;
    }

    return width;
}

static UINT32 ScaleChannel(UINT32 channel, UINT32 mask)
{
    UINT32 shift;
    UINT32 width;
    UINT32 maxValue;
    UINT32 scaled;

    if (mask == 0u)
    {
        return 0u;
    }

    shift = MaskShift(mask);
    width = MaskWidth(mask, shift);
    maxValue = (1u << width) - 1u;
    scaled = (channel * maxValue + 127u) / 255u;
    return (scaled << shift) & mask;
}

static UINT8 ExpandChannel(UINT32 channel, UINT32 width)
{
    UINT32 maxValue;

    if (width == 0u)
    {
        return 0u;
    }

    maxValue = (1u << width) - 1u;
    return (UINT8)((channel * 255u + (maxValue >> 1u)) / maxValue);
}

static UINT32 ComposePixel(UINT8 red, UINT8 green, UINT8 blue, UINT32 pixelFormat, UINT32 redMask, UINT32 greenMask, UINT32 blueMask)
{
    if (pixelFormat == GOP_PIXEL_RED_GREEN_BLUE_RESERVED_8BIT)
    {
        return ((UINT32)red) | ((UINT32)green << 8) | ((UINT32)blue << 16);
    }

    if (pixelFormat == GOP_PIXEL_BLUE_GREEN_RED_RESERVED_8BIT)
    {
        return ((UINT32)blue) | ((UINT32)green << 8) | ((UINT32)red << 16);
    }

    if (pixelFormat == GOP_PIXEL_BIT_MASK)
    {
        return ScaleChannel((UINT32)red, redMask)
            | ScaleChannel((UINT32)green, greenMask)
            | ScaleChannel((UINT32)blue, blueMask);
    }

    return 0u;
}

static void DecomposePixel(UINT32 pixel, UINT32 pixelFormat, UINT32 redMask, UINT32 greenMask, UINT32 blueMask, UINT8* redOut, UINT8* greenOut, UINT8* blueOut)
{
    if (pixelFormat == GOP_PIXEL_RED_GREEN_BLUE_RESERVED_8BIT)
    {
        *redOut = (UINT8)(pixel & 0xFFu);
        *greenOut = (UINT8)((pixel >> 8u) & 0xFFu);
        *blueOut = (UINT8)((pixel >> 16u) & 0xFFu);
        return;
    }

    if (pixelFormat == GOP_PIXEL_BLUE_GREEN_RED_RESERVED_8BIT)
    {
        *blueOut = (UINT8)(pixel & 0xFFu);
        *greenOut = (UINT8)((pixel >> 8u) & 0xFFu);
        *redOut = (UINT8)((pixel >> 16u) & 0xFFu);
        return;
    }

    if (pixelFormat == GOP_PIXEL_BIT_MASK)
    {
        UINT32 redShift = MaskShift(redMask);
        UINT32 greenShift = MaskShift(greenMask);
        UINT32 blueShift = MaskShift(blueMask);
        UINT32 redWidth = MaskWidth(redMask, redShift);
        UINT32 greenWidth = MaskWidth(greenMask, greenShift);
        UINT32 blueWidth = MaskWidth(blueMask, blueShift);

        *redOut = ExpandChannel((pixel & redMask) >> redShift, redWidth);
        *greenOut = ExpandChannel((pixel & greenMask) >> greenShift, greenWidth);
        *blueOut = ExpandChannel((pixel & blueMask) >> blueShift, blueWidth);
        return;
    }

    *redOut = 0u;
    *greenOut = 0u;
    *blueOut = 0u;
}

static UINT32 AlphaBlendPixel(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 destinationPixel, UINT8 red, UINT8 green, UINT8 blue, UINT8 alpha)
{
    UINT8 destinationRed;
    UINT8 destinationGreen;
    UINT8 destinationBlue;
    UINT8 outputRed;
    UINT8 outputGreen;
    UINT8 outputBlue;
    UINT32 inverseAlpha;

    if (alpha == 255u)
    {
        return ComposePixel(red, green, blue, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);
    }

    if (alpha == 0u)
    {
        return destinationPixel;
    }

    DecomposePixel(destinationPixel, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask, &destinationRed, &destinationGreen, &destinationBlue);
    inverseAlpha = 255u - (UINT32)alpha;
    outputRed = (UINT8)(((UINT32)red * (UINT32)alpha + (UINT32)destinationRed * inverseAlpha + 127u) / 255u);
    outputGreen = (UINT8)(((UINT32)green * (UINT32)alpha + (UINT32)destinationGreen * inverseAlpha + 127u) / 255u);
    outputBlue = (UINT8)(((UINT32)blue * (UINT32)alpha + (UINT32)destinationBlue * inverseAlpha + 127u) / 255u);

    return ComposePixel(outputRed, outputGreen, outputBlue, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);
}

static EFI_STATUS InitializeFramebuffer(void* systemTable, FRAMEBUFFER_INFO* framebufferInfo, EFI_STALL* stallOut)
{
    UINT8* bootServicesBytes;
    EFI_LOCATE_PROTOCOL locateProtocol;
    void* graphicsOutputProtocol;
    UINT8* modeBytes;
    UINT8* infoBytes;
    UINT64 framebufferSizeBytes;
    UINT32 maxPixels;
    UINT32 totalPixels;
    EFI_STATUS status;

    bootServicesBytes = *(UINT8**)((UINT8*)systemTable + EFI_SYSTEM_TABLE_BOOT_SERVICES_OFFSET);
    locateProtocol = *(EFI_LOCATE_PROTOCOL*)(bootServicesBytes + EFI_BOOT_SERVICES_LOCATE_PROTOCOL_OFFSET);
    *stallOut = *(EFI_STALL*)(bootServicesBytes + EFI_BOOT_SERVICES_STALL_OFFSET);

    graphicsOutputProtocol = 0;
    status = locateProtocol(&gGraphicsOutputProtocolGuid, 0, &graphicsOutputProtocol);
    if (status != EFI_SUCCESS || graphicsOutputProtocol == 0)
    {
        return (status != EFI_SUCCESS) ? status : EFI_UNSUPPORTED;
    }

    modeBytes = *(UINT8**)((UINT8*)graphicsOutputProtocol + GOP_MODE_OFFSET);
    infoBytes = *(UINT8**)(modeBytes + GOP_MODE_INFO_OFFSET);
    framebufferInfo->ScreenWidth = *(UINT32*)(infoBytes + GOP_INFO_HORIZONTAL_RESOLUTION_OFFSET);
    framebufferInfo->ScreenHeight = *(UINT32*)(infoBytes + GOP_INFO_VERTICAL_RESOLUTION_OFFSET);
    framebufferInfo->PixelFormat = *(UINT32*)(infoBytes + GOP_INFO_PIXEL_FORMAT_OFFSET);
    framebufferInfo->PixelsPerScanLine = *(UINT32*)(infoBytes + GOP_INFO_PIXELS_PER_SCANLINE_OFFSET);
    framebufferInfo->RedMask = *(UINT32*)(infoBytes + GOP_INFO_RED_MASK_OFFSET);
    framebufferInfo->GreenMask = *(UINT32*)(infoBytes + GOP_INFO_GREEN_MASK_OFFSET);
    framebufferInfo->BlueMask = *(UINT32*)(infoBytes + GOP_INFO_BLUE_MASK_OFFSET);

    if (framebufferInfo->PixelFormat == GOP_PIXEL_BLT_ONLY)
    {
        return EFI_UNSUPPORTED;
    }

    framebufferInfo->Framebuffer = (UINT32*)(UINTN)(*(UINT64*)(modeBytes + GOP_MODE_FRAMEBUFFER_BASE_OFFSET));
    framebufferSizeBytes = *(UINT64*)(modeBytes + GOP_MODE_FRAMEBUFFER_SIZE_OFFSET);
    maxPixels = (UINT32)(framebufferSizeBytes / 4u);
    totalPixels = framebufferInfo->PixelsPerScanLine * framebufferInfo->ScreenHeight;
    if (totalPixels > maxPixels)
    {
        return EFI_UNSUPPORTED;
    }

    return EFI_SUCCESS;
}

static void ClearScreen(const FRAMEBUFFER_INFO* Fb, UINT32 Color, UINT8 IgnoreBlackPixels)
{
    UINT32 Total;

    if (IgnoreBlackPixels != 0u && Color == 0u)
        return;

    Total = Fb->PixelsPerScanLine * Fb->ScreenHeight;
    FillPixels32(Fb->Framebuffer, Total, Color);
}

static void FillPixels32(UINT32* Destination, UINT32 Count, UINT32 Color)
{
    UINT32 Index;

    Index = 0u;
    if ((((UINTN)Destination) & 7u) == 0u)
    {
        UINT64 Color64;
        UINT64* Destination64;
        UINT32 PairIndex;
        UINT32 PairCount;
        UINT32 BulkPairCount;

        Color64 = ((UINT64)Color << 32) | (UINT64)Color;
        Destination64 = (UINT64*)Destination;
        PairCount = Count >> 1u;
        BulkPairCount = PairCount & ~7u;
        for (PairIndex = 0u; PairIndex < BulkPairCount; PairIndex += 8u)
        {
            Destination64[PairIndex + 0u] = Color64;
            Destination64[PairIndex + 1u] = Color64;
            Destination64[PairIndex + 2u] = Color64;
            Destination64[PairIndex + 3u] = Color64;
            Destination64[PairIndex + 4u] = Color64;
            Destination64[PairIndex + 5u] = Color64;
            Destination64[PairIndex + 6u] = Color64;
            Destination64[PairIndex + 7u] = Color64;
        }

        for (; PairIndex < PairCount; PairIndex++)
        {
            Destination64[PairIndex] = Color64;
        }

        Index = PairCount << 1u;
    }

    while (Index < Count)
    {
        Destination[Index] = Color;
        Index++;
    }
}

static void CopyPixels32(UINT32* Destination, const UINT32* Source, UINT32 Count)
{
    UINT32 Index;

    Index = 0u;
    if ((((UINTN)Destination | (UINTN)Source) & 7u) == 0u)
    {
        UINT64* Destination64;
        const UINT64* Source64;
        UINT32 PairIndex;
        UINT32 PairCount;
        UINT32 BulkPairCount;

        Destination64 = (UINT64*)Destination;
        Source64 = (const UINT64*)Source;
        PairCount = Count >> 1u;
        BulkPairCount = PairCount & ~7u;
        for (PairIndex = 0u; PairIndex < BulkPairCount; PairIndex += 8u)
        {
            Destination64[PairIndex + 0u] = Source64[PairIndex + 0u];
            Destination64[PairIndex + 1u] = Source64[PairIndex + 1u];
            Destination64[PairIndex + 2u] = Source64[PairIndex + 2u];
            Destination64[PairIndex + 3u] = Source64[PairIndex + 3u];
            Destination64[PairIndex + 4u] = Source64[PairIndex + 4u];
            Destination64[PairIndex + 5u] = Source64[PairIndex + 5u];
            Destination64[PairIndex + 6u] = Source64[PairIndex + 6u];
            Destination64[PairIndex + 7u] = Source64[PairIndex + 7u];
        }

        for (; PairIndex < PairCount; PairIndex++)
        {
            Destination64[PairIndex] = Source64[PairIndex];
        }

        Index = PairCount << 1u;
    }

    while (Index < Count)
    {
        Destination[Index] = Source[Index];
        Index++;
    }
}

static void BlitSurfaceRect(const FRAMEBUFFER_INFO* Destination, const FRAMEBUFFER_INFO* Source, UINT32 X, UINT32 Y, UINT32 Width, UINT32 Height)
{
    UINT32 Row;
    UINT32 MaxWidth;
    UINT32 MaxHeight;

    if (Destination == 0 || Source == 0 || Destination->Framebuffer == 0 || Source->Framebuffer == 0)
    {
        return;
    }

    if (X >= Destination->ScreenWidth || Y >= Destination->ScreenHeight ||
        X >= Source->ScreenWidth || Y >= Source->ScreenHeight)
    {
        return;
    }

    MaxWidth = Destination->ScreenWidth - X;
    if (Source->ScreenWidth - X < MaxWidth)
    {
        MaxWidth = Source->ScreenWidth - X;
    }

    MaxHeight = Destination->ScreenHeight - Y;
    if (Source->ScreenHeight - Y < MaxHeight)
    {
        MaxHeight = Source->ScreenHeight - Y;
    }

    if (Width > MaxWidth)
    {
        Width = MaxWidth;
    }

    if (Height > MaxHeight)
    {
        Height = MaxHeight;
    }

    for (Row = 0u; Row < Height; Row++)
    {
        UINT32 DestinationOffset = (Y + Row) * Destination->PixelsPerScanLine + X;
        UINT32 SourceOffset = (Y + Row) * Source->PixelsPerScanLine + X;
        CopyPixels32(Destination->Framebuffer + DestinationOffset, Source->Framebuffer + SourceOffset, Width);
    }
}

static UINT8 BlendChannelOverBlack(UINT8 channel, UINT8 alpha)
{
    return (UINT8)(((UINT32)channel * (UINT32)alpha + 127u) / 255u);
}

static void ComputeSplashRect(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 scale1024, UINT32* startXOut, UINT32* startYOut, UINT32* drawWidthOut, UINT32* drawHeightOut)
{
    UINT32 drawWidth = (SPLASH_WIDTH * scale1024) / 1024u;
    UINT32 drawHeight = (SPLASH_HEIGHT * scale1024) / 1024u;
    UINT32 startX;
    UINT32 startY;

    if (drawWidth > framebufferInfo->ScreenWidth)
    {
        drawWidth = framebufferInfo->ScreenWidth;
    }

    if (drawHeight > framebufferInfo->ScreenHeight)
    {
        drawHeight = framebufferInfo->ScreenHeight;
    }

    if (drawWidth == 0u || drawHeight == 0u)
    {
        startX = 0u;
        startY = 0u;
    }
    else
    {
        startX = (framebufferInfo->ScreenWidth > drawWidth) ? ((framebufferInfo->ScreenWidth - drawWidth) >> 1) : 0u;
        startY = (framebufferInfo->ScreenHeight > drawHeight) ? ((framebufferInfo->ScreenHeight - drawHeight) >> 1) : 0u;
    }

    if (startXOut != 0)
    {
        *startXOut = startX;
    }

    if (startYOut != 0)
    {
        *startYOut = startY;
    }

    if (drawWidthOut != 0)
    {
        *drawWidthOut = drawWidth;
    }

    if (drawHeightOut != 0)
    {
        *drawHeightOut = drawHeight;
    }
}

static void DrawSplashFrame(const FRAMEBUFFER_INFO* framebufferInfo, UINT8 alpha, UINT32 scale1024)
{
    UINT32 drawWidth;
    UINT32 drawHeight;
    UINT32 startX;
    UINT32 startY;
    UINT32 y;

    ComputeSplashRect(framebufferInfo, scale1024, &startX, &startY, &drawWidth, &drawHeight);
    if (drawWidth == 0u || drawHeight == 0u)
    {
        return;
    }

    for (y = 0u; y < drawHeight; y++)
    {
        UINT32 x;
        UINT32 framebufferRow = (startY + y) * framebufferInfo->PixelsPerScanLine + startX;
        UINT32 sourceY = (y * SPLASH_HEIGHT) / drawHeight;
        UINT32 sourceRow = sourceY * SPLASH_WIDTH * 3u;

        for (x = 0u; x < drawWidth; x++)
        {
            UINT32 sourceX = (x * SPLASH_WIDTH) / drawWidth;
            UINT32 sourceIndex = sourceRow + (sourceX * 3u);
            UINT8 red = BlendChannelOverBlack(gSplashPixels[sourceIndex + 0u], alpha);
            UINT8 green = BlendChannelOverBlack(gSplashPixels[sourceIndex + 1u], alpha);
            UINT8 blue = BlendChannelOverBlack(gSplashPixels[sourceIndex + 2u], alpha);
            framebufferInfo->Framebuffer[framebufferRow + x] = ComposePixel(
                red,
                green,
                blue,
                framebufferInfo->PixelFormat,
                framebufferInfo->RedMask,
                framebufferInfo->GreenMask,
                framebufferInfo->BlueMask);
        }
    }
}

static void PlaySplashSequence(const FRAMEBUFFER_INFO* framebufferInfo, EFI_STALL stall)
{
    UINT32 frame;
    const UINT32 frameCount = 30u;

    (void)stall(1000000u);

    for (frame = 0u; frame < frameCount; frame++)
    {
        UINT32 progress1024;
        UINT32 eased1024;
        UINT32 scale1024;
        UINT32 alpha1024;
        UINT8 alpha255;

        progress1024 = (frame * 1024u) / (frameCount - 1u);
        eased1024 = EvaluateTween(2u, 1u, progress1024);

        scale1024 = (975u * (1024u - eased1024) + 1024u * eased1024) / 1024u;
        alpha1024 = eased1024;
        alpha255 = (UINT8)((alpha1024 * 255u) / 1024u);

        ClearScreen(framebufferInfo, ComposePixel(0u, 0u, 0u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask), 1u);
        DrawSplashFrame(framebufferInfo, alpha255, scale1024);
        (void)stall(16667u);
    }

    (void)stall(1000000u);
}

static void InitializeCursorSurface(const FRAMEBUFFER_INFO* framebufferInfo, SOFTWARE_CURSOR* cursor)
{
    UINT32 Y;

    for (Y = 0u; Y < CURSOR_HEIGHT; Y++)
    {
        UINT32 X;
        UINT32 CursorRow = Y * CURSOR_WIDTH;
        UINT32 PixelRow = CursorRow * 3u;

        for (X = 0u; X < CURSOR_WIDTH; X++)
        {
            UINT32 PixelIndex = CursorRow + X;
            UINT32 SourceIndex = PixelRow + (X * 3u);
            UINT8 Alpha = gCursorAlpha[PixelIndex];

            if (Alpha == 0u)
            {
                cursor->OpaqueMask[PixelIndex] = 0u;
                cursor->ResolvedPixels[PixelIndex] = 0u;
                continue;
            }

            cursor->OpaqueMask[PixelIndex] = 1u;
            cursor->ResolvedPixels[PixelIndex] = ComposePixel(
                BlendChannelOverBlack(gCursorPixels[SourceIndex + 0u], Alpha),
                BlendChannelOverBlack(gCursorPixels[SourceIndex + 1u], Alpha),
                BlendChannelOverBlack(gCursorPixels[SourceIndex + 2u], Alpha),
                framebufferInfo->PixelFormat,
                framebufferInfo->RedMask,
                framebufferInfo->GreenMask,
                framebufferInfo->BlueMask);
        }
    }
}

static void RestoreCursorBackground(const FRAMEBUFFER_INFO* framebufferInfo, SOFTWARE_CURSOR* cursor)
{
    UINT32 y;

    if (cursor->Visible == 0u)
    {
        return;
    }

    for (y = 0u; y < cursor->SavedHeight; y++)
    {
        UINT32 framebufferRow = (cursor->Y + y) * framebufferInfo->PixelsPerScanLine + cursor->X;
        UINT32 savedRow = y * CURSOR_WIDTH;
        CopyPixels32(
            framebufferInfo->Framebuffer + framebufferRow,
            cursor->SavedPixels + savedRow,
            cursor->SavedWidth);
    }

    cursor->Visible = 0u;
}

static void SaveCursorBackground(const FRAMEBUFFER_INFO* framebufferInfo, SOFTWARE_CURSOR* cursor)
{
    UINT32 y;

    cursor->SavedWidth = CURSOR_WIDTH;
    cursor->SavedHeight = CURSOR_HEIGHT;

    if (cursor->X + cursor->SavedWidth > framebufferInfo->ScreenWidth)
    {
        cursor->SavedWidth = framebufferInfo->ScreenWidth - cursor->X;
    }

    if (cursor->Y + cursor->SavedHeight > framebufferInfo->ScreenHeight)
    {
        cursor->SavedHeight = framebufferInfo->ScreenHeight - cursor->Y;
    }

    for (y = 0u; y < cursor->SavedHeight; y++)
    {
        UINT32 framebufferRow = (cursor->Y + y) * framebufferInfo->PixelsPerScanLine + cursor->X;
        UINT32 savedRow = y * CURSOR_WIDTH;
        CopyPixels32(
            cursor->SavedPixels + savedRow,
            framebufferInfo->Framebuffer + framebufferRow,
            cursor->SavedWidth);
    }
}

static void DrawCursor(const FRAMEBUFFER_INFO* FramebufferInfo, const SOFTWARE_CURSOR* Cursor)
{
    UINT32 Y;

    for (Y = 0u; Y < Cursor->SavedHeight; Y++)
    {
        UINT32 X;
        UINT32 FramebufferRow = (Cursor->Y + Y) * FramebufferInfo->PixelsPerScanLine + Cursor->X;
        UINT32 CursorRow = Y * CURSOR_WIDTH;

        for (X = 0u; X < Cursor->SavedWidth; X++)
        {
            UINT32 PixelIndex = CursorRow + X;
            if (Cursor->OpaqueMask[PixelIndex] == 0u)
                continue;

            FramebufferInfo->Framebuffer[FramebufferRow + X] = Cursor->ResolvedPixels[PixelIndex];
        }
    }
}

static void ClearRect(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 x, UINT32 y, UINT32 width, UINT32 height, UINT32 color)
{
    UINT32 row;
    UINT32 maxWidth;
    UINT32 maxHeight;

    if (x >= framebufferInfo->ScreenWidth || y >= framebufferInfo->ScreenHeight)
    {
        return;
    }

    maxWidth = framebufferInfo->ScreenWidth - x;
    maxHeight = framebufferInfo->ScreenHeight - y;
    if (width > maxWidth)
    {
        width = maxWidth;
    }

    if (height > maxHeight)
    {
        height = maxHeight;
    }

    for (row = 0u; row < height; row++)
    {
        UINT32 offset = (y + row) * framebufferInfo->PixelsPerScanLine + x;
        FillPixels32(framebufferInfo->Framebuffer + offset, width, color);
    }
}

static UINT8 PointInRoundedBox(UINT32 x, UINT32 y, const UI_BOX* box)
{
    INT32 dx;
    INT32 dy;
    INT32 localX;
    INT32 localY;
    INT32 radius;

    if (x < box->X || y < box->Y || x >= box->X + box->Width || y >= box->Y + box->Height)
    {
        return 0u;
    }

    radius = (INT32)box->Radius;
    if (radius <= 0)
    {
        return 1u;
    }

    localX = (INT32)(x - box->X);
    localY = (INT32)(y - box->Y);
    if ((UINT32)localX >= box->Radius && (UINT32)localX < box->Width - box->Radius)
    {
        return 1u;
    }

    if ((UINT32)localY >= box->Radius && (UINT32)localY < box->Height - box->Radius)
    {
        return 1u;
    }

    dx = (localX < radius) ? (radius - 1 - localX) : (localX - ((INT32)box->Width - radius));
    dy = (localY < radius) ? (radius - 1 - localY) : (localY - ((INT32)box->Height - radius));
    return (UINT8)(((dx * dx) + (dy * dy)) < (radius * radius));
}

static void DrawRoundedBox(const FRAMEBUFFER_INFO* framebufferInfo, const UI_BOX* box)
{
    UINT32 row;
    UINT32 column;

    for (row = 0u; row < box->Height; row++)
    {
        for (column = 0u; column < box->Width; column++)
        {
            UINT32 pixelX = box->X + column;
            UINT32 pixelY = box->Y + row;
            if (pixelX >= framebufferInfo->ScreenWidth || pixelY >= framebufferInfo->ScreenHeight)
            {
                continue;
            }

            if (PointInRoundedBox(pixelX, pixelY, box) != 0u)
            {
                framebufferInfo->Framebuffer[pixelY * framebufferInfo->PixelsPerScanLine + pixelX] = box->Color;
            }
        }
    }
}

static void DrawGlyph(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT8 glyph, UINT8 red, UINT8 green, UINT8 blue)
{
    UINT32 glyphIndex;
    UINT32 row;

    if (glyph < FONT_FIRST_CHAR || glyph >= FONT_FIRST_CHAR + FONT_GLYPH_COUNT)
    {
        return;
    }

    glyphIndex = (UINT32)(glyph - FONT_FIRST_CHAR);
    for (row = 0u; row < FONT_GLYPH_HEIGHT; row++)
    {
        UINT32 column;
        INT32 pixelY = y + (INT32)row;
        if (pixelY < 0 || pixelY >= (INT32)framebufferInfo->ScreenHeight)
        {
            continue;
        }

        for (column = 0u; column < FONT_GLYPH_WIDTH; column++)
        {
            UINT8 alpha;
            INT32 pixelX = x + (INT32)column;
            UINT32 alphaIndex;

            if (pixelX < 0 || pixelX >= (INT32)framebufferInfo->ScreenWidth)
            {
                continue;
            }

            alphaIndex = (glyphIndex * FONT_GLYPH_WIDTH * FONT_GLYPH_HEIGHT) + (row * FONT_GLYPH_WIDTH) + column;
            alpha = gFontGlyphAlpha[alphaIndex];
            if (alpha == 0u)
            {
                continue;
            }

            framebufferInfo->Framebuffer[(UINT32)pixelY * framebufferInfo->PixelsPerScanLine + (UINT32)pixelX] =
                AlphaBlendPixel(
                    framebufferInfo,
                    framebufferInfo->Framebuffer[(UINT32)pixelY * framebufferInfo->PixelsPerScanLine + (UINT32)pixelX],
                    red,
                    green,
                    blue,
                    alpha);
        }
    }
}

static void DrawTextString(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 red, UINT8 green, UINT8 blue)
{
    INT32 cursorX;

    if (text == 0)
    {
        return;
    }

    cursorX = x;
    while (*text != 0)
    {
        UINT8 glyph = (UINT8)(*text);
        if (glyph == (UINT8)'\n')
        {
            cursorX = x;
            y += (INT32)FONT_GLYPH_HEIGHT;
            text++;
            continue;
        }

        DrawGlyph(framebufferInfo, cursorX, y, glyph, red, green, blue);
        if (glyph >= FONT_FIRST_CHAR && glyph < FONT_FIRST_CHAR + FONT_GLYPH_COUNT)
        {
            cursorX += (INT32)gFontGlyphAdvance[glyph - FONT_FIRST_CHAR];
        }
        else
        {
            cursorX += 8;
        }

        text++;
    }
}

static void DrawGlyphAtlas(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT8 glyph, UINT8 red, UINT8 green, UINT8 blue, UINT32 firstChar, UINT32 glyphCount, UINT32 glyphWidth, UINT32 glyphHeight, const UINT8* glyphAlpha)
{
    UINT32 glyphIndex;
    UINT32 row;

    if (glyphAlpha == 0 || glyph < firstChar || glyph >= firstChar + glyphCount)
    {
        return;
    }

    glyphIndex = (UINT32)(glyph - firstChar);
    for (row = 0u; row < glyphHeight; row++)
    {
        UINT32 column;
        INT32 pixelY = y + (INT32)row;
        if (pixelY < 0 || pixelY >= (INT32)framebufferInfo->ScreenHeight)
        {
            continue;
        }

        for (column = 0u; column < glyphWidth; column++)
        {
            UINT8 alpha;
            INT32 pixelX = x + (INT32)column;
            UINT32 alphaIndex;

            if (pixelX < 0 || pixelX >= (INT32)framebufferInfo->ScreenWidth)
            {
                continue;
            }

            alphaIndex = (glyphIndex * glyphWidth * glyphHeight) + (row * glyphWidth) + column;
            alpha = glyphAlpha[alphaIndex];
            if (alpha == 0u)
            {
                continue;
            }

            framebufferInfo->Framebuffer[(UINT32)pixelY * framebufferInfo->PixelsPerScanLine + (UINT32)pixelX] =
                AlphaBlendPixel(
                    framebufferInfo,
                    framebufferInfo->Framebuffer[(UINT32)pixelY * framebufferInfo->PixelsPerScanLine + (UINT32)pixelX],
                    red,
                    green,
                    blue,
                    alpha);
        }
    }
}

static void DrawTextStringAtlas(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 red, UINT8 green, UINT8 blue, UINT32 firstChar, UINT32 glyphCount, UINT32 glyphWidth, UINT32 glyphHeight, const UINT8* glyphAlpha, const UINT8* glyphAdvance)
{
    INT32 cursorX;

    if (text == 0 || glyphAdvance == 0)
    {
        return;
    }

    cursorX = x;
    while (*text != 0)
    {
        UINT8 glyph = (UINT8)(*text);
        if (glyph == (UINT8)'\n')
        {
            cursorX = x;
            y += (INT32)glyphHeight;
            text++;
            continue;
        }

        DrawGlyphAtlas(framebufferInfo, cursorX, y, glyph, red, green, blue, firstChar, glyphCount, glyphWidth, glyphHeight, glyphAlpha);
        if (glyph >= firstChar && glyph < firstChar + glyphCount)
        {
            cursorX += (INT32)glyphAdvance[glyph - firstChar];
        }
        else
        {
            cursorX += (INT32)(glyphWidth / 2u);
        }

        text++;
    }
}

static INT32 MeasureTextStringAtlas(const char* text, UINT32 firstChar, UINT32 glyphCount, const UINT8* glyphAdvance, UINT32 fallbackAdvance)
{
    INT32 lineWidth;
    INT32 maxWidth;

    if (text == 0 || glyphAdvance == 0)
    {
        return 0;
    }

    lineWidth = 0;
    maxWidth = 0;
    while (*text != 0)
    {
        UINT8 glyph = (UINT8)(*text);
        if (glyph == (UINT8)'\n')
        {
            if (lineWidth > maxWidth)
            {
                maxWidth = lineWidth;
            }

            lineWidth = 0;
            text++;
            continue;
        }

        if (glyph >= firstChar && glyph < firstChar + glyphCount)
        {
            lineWidth += (INT32)glyphAdvance[glyph - firstChar];
        }
        else
        {
            lineWidth += (INT32)fallbackAdvance;
        }

        text++;
    }

    if (lineWidth > maxWidth)
    {
        maxWidth = lineWidth;
    }

    return maxWidth;
}

static void DrawGradientOutlinedTextAtlas(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 startRed, UINT8 startGreen, UINT8 startBlue, UINT8 endRed, UINT8 endGreen, UINT8 endBlue, UINT32 outlineThickness, UINT32 firstChar, UINT32 glyphCount, UINT32 glyphWidth, UINT32 glyphHeight, const UINT8* glyphAlpha, const UINT8* glyphAdvance, UINT32 fallbackAdvance)
{
    INT32 totalWidth;
    INT32 cursorX;

    if (text == 0)
    {
        return;
    }

    totalWidth = MeasureTextStringAtlas(text, firstChar, glyphCount, glyphAdvance, fallbackAdvance);
    if (outlineThickness != 0u)
    {
        INT32 offsetY;
        for (offsetY = -(INT32)outlineThickness; offsetY <= (INT32)outlineThickness; offsetY++)
        {
            INT32 offsetX;
            for (offsetX = -(INT32)outlineThickness; offsetX <= (INT32)outlineThickness; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0)
                {
                    continue;
                }

                DrawTextStringAtlas(framebufferInfo, x + offsetX, y + offsetY, text, 0u, 0u, 0u, firstChar, glyphCount, glyphWidth, glyphHeight, glyphAlpha, glyphAdvance);
            }
        }
    }

    cursorX = x;
    while (*text != 0)
    {
        UINT8 glyph = (UINT8)(*text);
        UINT8 red;
        UINT8 green;
        UINT8 blue;
        UINT32 progress1024;
        INT32 glyphAdvancePixels;
        INT32 glyphCenter;

        if (glyph == (UINT8)'\n')
        {
            cursorX = x;
            y += (INT32)glyphHeight;
            text++;
            continue;
        }

        glyphAdvancePixels = (glyph >= firstChar && glyph < firstChar + glyphCount)
            ? (INT32)glyphAdvance[glyph - firstChar]
            : (INT32)fallbackAdvance;
        glyphCenter = cursorX - x + (glyphAdvancePixels / 2);
        progress1024 = totalWidth > 0 ? (UINT32)((glyphCenter * 1024) / totalWidth) : 0u;
        if (progress1024 > 1024u)
        {
            progress1024 = 1024u;
        }

        red = (UINT8)(startRed + (((INT32)endRed - (INT32)startRed) * (INT32)progress1024) / 1024);
        green = (UINT8)(startGreen + (((INT32)endGreen - (INT32)startGreen) * (INT32)progress1024) / 1024);
        blue = (UINT8)(startBlue + (((INT32)endBlue - (INT32)startBlue) * (INT32)progress1024) / 1024);
        DrawGlyphAtlas(framebufferInfo, cursorX, y, glyph, red, green, blue, firstChar, glyphCount, glyphWidth, glyphHeight, glyphAlpha);
        cursorX += glyphAdvancePixels;
        text++;
    }
}

static void DrawGlyphScaled(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT8 glyph, UINT8 red, UINT8 green, UINT8 blue, UINT32 scale)
{
    UINT32 glyphIndex;
    UINT32 row;

    if (scale == 0u || glyph < FONT_FIRST_CHAR || glyph >= FONT_FIRST_CHAR + FONT_GLYPH_COUNT)
    {
        return;
    }

    glyphIndex = (UINT32)(glyph - FONT_FIRST_CHAR);
    for (row = 0u; row < FONT_GLYPH_HEIGHT; row++)
    {
        UINT32 column;
        for (column = 0u; column < FONT_GLYPH_WIDTH; column++)
        {
            UINT8 alpha;
            UINT32 alphaIndex;
            UINT32 scaleY;

            alphaIndex = (glyphIndex * FONT_GLYPH_WIDTH * FONT_GLYPH_HEIGHT) + (row * FONT_GLYPH_WIDTH) + column;
            alpha = gFontGlyphAlpha[alphaIndex];
            if (alpha == 0u)
            {
                continue;
            }

            for (scaleY = 0u; scaleY < scale; scaleY++)
            {
                UINT32 scaleX;
                INT32 pixelY = y + (INT32)(row * scale + scaleY);
                if (pixelY < 0 || pixelY >= (INT32)framebufferInfo->ScreenHeight)
                {
                    continue;
                }

                for (scaleX = 0u; scaleX < scale; scaleX++)
                {
                    INT32 pixelX = x + (INT32)(column * scale + scaleX);
                    UINT32 destinationIndex;

                    if (pixelX < 0 || pixelX >= (INT32)framebufferInfo->ScreenWidth)
                    {
                        continue;
                    }

                    destinationIndex = (UINT32)pixelY * framebufferInfo->PixelsPerScanLine + (UINT32)pixelX;
                    framebufferInfo->Framebuffer[destinationIndex] =
                        AlphaBlendPixel(framebufferInfo, framebufferInfo->Framebuffer[destinationIndex], red, green, blue, alpha);
                }
            }
        }
    }
}

static void DrawTextStringScaled(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 red, UINT8 green, UINT8 blue, UINT32 scale)
{
    INT32 cursorX;

    if (text == 0 || scale == 0u)
    {
        return;
    }

    cursorX = x;
    while (*text != 0)
    {
        UINT8 glyph = (UINT8)(*text);
        if (glyph == (UINT8)'\n')
        {
            cursorX = x;
            y += (INT32)(FONT_GLYPH_HEIGHT * scale);
            text++;
            continue;
        }

        DrawGlyphScaled(framebufferInfo, cursorX, y, glyph, red, green, blue, scale);
        if (glyph >= FONT_FIRST_CHAR && glyph < FONT_FIRST_CHAR + FONT_GLYPH_COUNT)
        {
            cursorX += (INT32)(gFontGlyphAdvance[glyph - FONT_FIRST_CHAR] * scale);
        }
        else
        {
            cursorX += (INT32)(8u * scale);
        }

        text++;
    }
}

static INT32 MeasureTextStringScaled(const char* text, UINT32 scale)
{
    INT32 lineWidth;
    INT32 maxWidth;

    if (text == 0 || scale == 0u)
    {
        return 0;
    }

    lineWidth = 0;
    maxWidth = 0;
    while (*text != 0)
    {
        UINT8 glyph = (UINT8)(*text);
        if (glyph == (UINT8)'\n')
        {
            if (lineWidth > maxWidth)
            {
                maxWidth = lineWidth;
            }

            lineWidth = 0;
            text++;
            continue;
        }

        if (glyph >= FONT_FIRST_CHAR && glyph < FONT_FIRST_CHAR + FONT_GLYPH_COUNT)
        {
            lineWidth += (INT32)(gFontGlyphAdvance[glyph - FONT_FIRST_CHAR] * scale);
        }
        else
        {
            lineWidth += (INT32)(8u * scale);
        }

        text++;
    }

    if (lineWidth > maxWidth)
    {
        maxWidth = lineWidth;
    }

    return maxWidth;
}

static void DrawGradientOutlinedTextScaled(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT32 scale, UINT8 startRed, UINT8 startGreen, UINT8 startBlue, UINT8 endRed, UINT8 endGreen, UINT8 endBlue, UINT32 outlineThickness)
{
    INT32 totalWidth;
    INT32 cursorX;

    if (text == 0 || scale == 0u)
    {
        return;
    }

    totalWidth = MeasureTextStringScaled(text, scale);
    if (outlineThickness != 0u)
    {
        INT32 offsetY;
        for (offsetY = -(INT32)outlineThickness; offsetY <= (INT32)outlineThickness; offsetY++)
        {
            INT32 offsetX;
            for (offsetX = -(INT32)outlineThickness; offsetX <= (INT32)outlineThickness; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0)
                {
                    continue;
                }

                DrawTextStringScaled(framebufferInfo, x + offsetX, y + offsetY, text, 0u, 0u, 0u, scale);
            }
        }
    }

    cursorX = x;
    while (*text != 0)
    {
        UINT8 glyph = (UINT8)(*text);
        UINT8 red;
        UINT8 green;
        UINT8 blue;
        UINT32 progress1024;
        INT32 glyphAdvance;
        INT32 glyphCenter;

        if (glyph == (UINT8)'\n')
        {
            cursorX = x;
            y += (INT32)(FONT_GLYPH_HEIGHT * scale);
            text++;
            continue;
        }

        glyphAdvance = (glyph >= FONT_FIRST_CHAR && glyph < FONT_FIRST_CHAR + FONT_GLYPH_COUNT)
            ? (INT32)(gFontGlyphAdvance[glyph - FONT_FIRST_CHAR] * scale)
            : (INT32)(8u * scale);
        glyphCenter = cursorX - x + (glyphAdvance / 2);
        progress1024 = totalWidth > 0 ? (UINT32)((glyphCenter * 1024) / totalWidth) : 0u;
        if (progress1024 > 1024u)
        {
            progress1024 = 1024u;
        }

        red = (UINT8)(startRed + (((INT32)endRed - (INT32)startRed) * (INT32)progress1024) / 1024);
        green = (UINT8)(startGreen + (((INT32)endGreen - (INT32)startGreen) * (INT32)progress1024) / 1024);
        blue = (UINT8)(startBlue + (((INT32)endBlue - (INT32)startBlue) * (INT32)progress1024) / 1024);
        DrawGlyphScaled(framebufferInfo, cursorX, y, glyph, red, green, blue, scale);
        cursorX += glyphAdvance;
        text++;
    }
}

static void DrawIconGlyph(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT8 glyph, UINT8 red, UINT8 green, UINT8 blue)
{
    UINT32 glyphIndex;
    UINT32 row;

    if (glyph < ICON_FONT_FIRST_CHAR || glyph >= ICON_FONT_FIRST_CHAR + ICON_FONT_GLYPH_COUNT)
    {
        return;
    }

    glyphIndex = (UINT32)(glyph - ICON_FONT_FIRST_CHAR);
    for (row = 0u; row < ICON_FONT_GLYPH_HEIGHT; row++)
    {
        UINT32 column;
        INT32 pixelY = y + (INT32)row;
        if (pixelY < 0 || pixelY >= (INT32)framebufferInfo->ScreenHeight)
        {
            continue;
        }

        for (column = 0u; column < ICON_FONT_GLYPH_WIDTH; column++)
        {
            UINT8 alpha;
            INT32 pixelX = x + (INT32)column;
            UINT32 alphaIndex;

            if (pixelX < 0 || pixelX >= (INT32)framebufferInfo->ScreenWidth)
            {
                continue;
            }

            alphaIndex = (glyphIndex * ICON_FONT_GLYPH_WIDTH * ICON_FONT_GLYPH_HEIGHT) + (row * ICON_FONT_GLYPH_WIDTH) + column;
            alpha = gIconFontGlyphAlpha[alphaIndex];
            if (alpha == 0u)
            {
                continue;
            }

            framebufferInfo->Framebuffer[(UINT32)pixelY * framebufferInfo->PixelsPerScanLine + (UINT32)pixelX] =
                AlphaBlendPixel(
                    framebufferInfo,
                    framebufferInfo->Framebuffer[(UINT32)pixelY * framebufferInfo->PixelsPerScanLine + (UINT32)pixelX],
                    red,
                    green,
                    blue,
                    alpha);
        }
    }
}

static void DrawIconTextString(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text, UINT8 red, UINT8 green, UINT8 blue)
{
    INT32 cursorX;

    if (text == 0)
    {
        return;
    }

    cursorX = x;
    while (*text != 0)
    {
        UINT8 glyph = (UINT8)(*text);
        if (glyph == (UINT8)'\n')
        {
            cursorX = x;
            y += (INT32)ICON_FONT_GLYPH_HEIGHT;
            text++;
            continue;
        }

        DrawIconGlyph(framebufferInfo, cursorX, y, glyph, red, green, blue);
        if (glyph >= ICON_FONT_FIRST_CHAR && glyph < ICON_FONT_FIRST_CHAR + ICON_FONT_GLYPH_COUNT)
        {
            cursorX += (INT32)gIconFontGlyphAdvance[glyph - ICON_FONT_FIRST_CHAR];
        }
        else
        {
            cursorX += 4;
        }

        text++;
    }
}

static void DrawImageNearestAlpha(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, UINT32 drawWidth, UINT32 drawHeight, UINT32 sourceWidth, UINT32 sourceHeight, const UINT8* pixels, const UINT8* alpha)
{
    UINT32 row;

    if (drawWidth == 0u || drawHeight == 0u || sourceWidth == 0u || sourceHeight == 0u || pixels == 0 || alpha == 0)
    {
        return;
    }

    for (row = 0u; row < drawHeight; row++)
    {
        UINT32 sourceYFixed = drawHeight > 1u
            ? (row * (sourceHeight - 1u) * 256u) / (drawHeight - 1u)
            : 0u;
        UINT32 sourceY = sourceYFixed >> 8u;
        UINT32 sourceYNext = sourceY + 1u < sourceHeight ? sourceY + 1u : sourceY;
        UINT32 blendY = sourceYFixed & 0xFFu;
        INT32 pixelY = y + (INT32)row;
        UINT32 column;

        if (pixelY < 0 || pixelY >= (INT32)framebufferInfo->ScreenHeight)
        {
            continue;
        }

        for (column = 0u; column < drawWidth; column++)
        {
            UINT32 sourceXFixed = drawWidth > 1u
                ? (column * (sourceWidth - 1u) * 256u) / (drawWidth - 1u)
                : 0u;
            UINT32 sourceX = sourceXFixed >> 8u;
            UINT32 sourceXNext = sourceX + 1u < sourceWidth ? sourceX + 1u : sourceX;
            UINT32 blendX = sourceXFixed & 0xFFu;
            UINT32 sourceIndex00 = sourceY * sourceWidth + sourceX;
            UINT32 sourceIndex10 = sourceY * sourceWidth + sourceXNext;
            UINT32 sourceIndex01 = sourceYNext * sourceWidth + sourceX;
            UINT32 sourceIndex11 = sourceYNext * sourceWidth + sourceXNext;
            UINT32 weight00 = (256u - blendX) * (256u - blendY);
            UINT32 weight10 = blendX * (256u - blendY);
            UINT32 weight01 = (256u - blendX) * blendY;
            UINT32 weight11 = blendX * blendY;
            UINT32 sourceAlphaWeighted;
            UINT8 sourceAlpha;
            INT32 pixelX = x + (INT32)column;
            UINT32 destinationIndex;
            UINT32 red;
            UINT32 green;
            UINT32 blue;

            if (pixelX < 0 || pixelX >= (INT32)framebufferInfo->ScreenWidth)
            {
                continue;
            }

            sourceAlphaWeighted =
                (UINT32)alpha[sourceIndex00] * weight00 +
                (UINT32)alpha[sourceIndex10] * weight10 +
                (UINT32)alpha[sourceIndex01] * weight01 +
                (UINT32)alpha[sourceIndex11] * weight11;
            sourceAlpha = (UINT8)((sourceAlphaWeighted + 32768u) >> 16u);
            if (sourceAlpha == 0u)
            {
                continue;
            }

            red =
                (UINT32)pixels[sourceIndex00 * 3u + 0u] * weight00 +
                (UINT32)pixels[sourceIndex10 * 3u + 0u] * weight10 +
                (UINT32)pixels[sourceIndex01 * 3u + 0u] * weight01 +
                (UINT32)pixels[sourceIndex11 * 3u + 0u] * weight11;
            green =
                (UINT32)pixels[sourceIndex00 * 3u + 1u] * weight00 +
                (UINT32)pixels[sourceIndex10 * 3u + 1u] * weight10 +
                (UINT32)pixels[sourceIndex01 * 3u + 1u] * weight01 +
                (UINT32)pixels[sourceIndex11 * 3u + 1u] * weight11;
            blue =
                (UINT32)pixels[sourceIndex00 * 3u + 2u] * weight00 +
                (UINT32)pixels[sourceIndex10 * 3u + 2u] * weight10 +
                (UINT32)pixels[sourceIndex01 * 3u + 2u] * weight01 +
                (UINT32)pixels[sourceIndex11 * 3u + 2u] * weight11;

            destinationIndex = (UINT32)pixelY * framebufferInfo->PixelsPerScanLine + (UINT32)pixelX;
            framebufferInfo->Framebuffer[destinationIndex] =
                AlphaBlendPixel(
                    framebufferInfo,
                    framebufferInfo->Framebuffer[destinationIndex],
                    (UINT8)((red + 32768u) >> 16u),
                    (UINT8)((green + 32768u) >> 16u),
                    (UINT8)((blue + 32768u) >> 16u),
                    sourceAlpha);
        }
    }
}

static void BlitCursorComposite(const FRAMEBUFFER_INFO* destinationFramebufferInfo, const FRAMEBUFFER_INFO* shadowFramebufferInfo, FRAMEBUFFER_INFO* composeFramebufferInfo, SOFTWARE_CURSOR* cursor)
{
    UINT32 y;

    if (destinationFramebufferInfo == 0 || shadowFramebufferInfo == 0 || cursor == 0)
    {
        return;
    }

    SaveCursorBackground(shadowFramebufferInfo, cursor);

    if (composeFramebufferInfo != 0 &&
        composeFramebufferInfo->Framebuffer != 0 &&
        composeFramebufferInfo->Framebuffer != destinationFramebufferInfo->Framebuffer &&
        composeFramebufferInfo->Framebuffer != shadowFramebufferInfo->Framebuffer)
    {
        for (y = 0u; y < cursor->SavedHeight; y++)
        {
            UINT32 framebufferRow = (cursor->Y + y) * composeFramebufferInfo->PixelsPerScanLine + cursor->X;
            UINT32 savedRow = y * CURSOR_WIDTH;
            CopyPixels32(
                composeFramebufferInfo->Framebuffer + framebufferRow,
                cursor->SavedPixels + savedRow,
                cursor->SavedWidth);
        }

        DrawCursor(composeFramebufferInfo, cursor);
        BlitSurfaceRect(
            destinationFramebufferInfo,
            composeFramebufferInfo,
            cursor->X,
            cursor->Y,
            cursor->SavedWidth,
            cursor->SavedHeight);
    }
    else
    {
        DrawCursor(destinationFramebufferInfo, cursor);
    }

    cursor->Visible = 1u;
}

static UINT16 EvaluateTween(UINT32 tweenFamily, UINT32 tweenMode, UINT32 progress1024)
{
    const UINT16* table = gTweenLinearIn;
    if (progress1024 > 1024u)
    {
        progress1024 = 1024u;
    }

    switch (tweenFamily)
    {
        case 1u:
            table = tweenMode == 0u ? gTweenSineIn
                : tweenMode == 1u ? gTweenSineOut
                : tweenMode == 2u ? gTweenSineInOut
                : gTweenSineOutIn;
            break;
        case 2u:
            table = tweenMode == 0u ? gTweenCubicIn
                : tweenMode == 1u ? gTweenCubicOut
                : tweenMode == 2u ? gTweenCubicInOut
                : gTweenCubicOutIn;
            break;
        case 3u:
            table = tweenMode == 0u ? gTweenExponentialIn
                : tweenMode == 1u ? gTweenExponentialOut
                : tweenMode == 2u ? gTweenExponentialInOut
                : gTweenExponentialOutIn;
            break;
        case 4u:
            table = tweenMode == 0u ? gTweenBackIn
                : tweenMode == 1u ? gTweenBackOut
                : tweenMode == 2u ? gTweenBackInOut
                : gTweenBackOutIn;
            break;
        default:
            table = tweenMode == 0u ? gTweenLinearIn
                : tweenMode == 1u ? gTweenLinearOut
                : tweenMode == 2u ? gTweenLinearInOut
                : gTweenLinearOutIn;
            break;
    }

    return table[progress1024];
}

static void AppendText(char** cursor, const char* text)
{
    while (*text != 0)
    {
        **cursor = *text;
        (*cursor)++;
        text++;
    }
}

static void AppendInt(char** cursor, INT32 value)
{
    char digits[16];
    UINT32 magnitude;
    UINT32 count;

    if (value == 0)
    {
        **cursor = '0';
        (*cursor)++;
        return;
    }

    if (value < 0)
    {
        **cursor = '-';
        (*cursor)++;
        magnitude = (UINT32)(-value);
    }
    else
    {
        magnitude = (UINT32)value;
    }

    count = 0u;
    while (magnitude > 0u)
    {
        digits[count++] = (char)('0' + (magnitude % 10u));
        magnitude /= 10u;
    }

    while (count > 0u)
    {
        count--;
        **cursor = digits[count];
        (*cursor)++;
    }
}

static void AppendUIntHexFixed(char** cursor, UINT32 value, UINT32 digits)
{
    UINT32 index;

    for (index = 0u; index < digits; index++)
    {
        UINT32 shift = (digits - 1u - index) * 4u;
        UINT32 nibble = (value >> shift) & 0xFu;
        **cursor = (char)(nibble < 10u ? ('0' + nibble) : ('A' + (nibble - 10u)));
        (*cursor)++;
    }
}

static UINT32 CountTextLines(const char* text)
{
    UINT32 lines;

    if (text == 0 || *text == 0)
    {
        return 0u;
    }

    lines = 1u;
    while (*text != 0)
    {
        if (*text == '\n')
        {
            lines++;
        }

        text++;
    }

    return lines;
}

static UINT32 CountIconTextLines(const char* text)
{
    return CountTextLines(text);
}

static INT32 AdvanceConsoleBlockY(INT32 y, const char* text)
{
    return y + (INT32)(CountTextLines(text) * FONT_GLYPH_HEIGHT) + 5;
}

static INT32 AdvanceIconConsoleBlockY(INT32 y, const char* text)
{
    return y + (INT32)(CountIconTextLines(text) * ICON_FONT_GLYPH_HEIGHT) + 5;
}

static INT32 DrawConsoleBlock(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text)
{
    UINT32 lines;

    if (text == 0 || *text == 0)
    {
        return y;
    }

    DrawTextString(framebufferInfo, x, y, text, 255u, 255u, 255u);
    lines = CountTextLines(text);
    return y + (INT32)(lines * FONT_GLYPH_HEIGHT) + 5;
}

static INT32 DrawIconConsoleBlock(const FRAMEBUFFER_INFO* framebufferInfo, INT32 x, INT32 y, const char* text)
{
    UINT32 lines;

    if (text == 0 || *text == 0)
    {
        return y;
    }

    DrawIconTextString(framebufferInfo, x, y, text, 255u, 255u, 255u);
    lines = CountIconTextLines(text);
    return y + (INT32)(lines * ICON_FONT_GLYPH_HEIGHT) + 5;
}

static void DrawMouseDebug(const FRAMEBUFFER_INFO* framebufferInfo, const SOFTWARE_CURSOR* cursor)
{
    char line[96];
    char* writeCursor;
    UINT32 black;
    UINT32 debugY;
    UINT32 debugHeight;

    black = ComposePixel(0u, 0u, 0u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);
    debugHeight = (FONT_GLYPH_HEIGHT * 2u) + 8u;
    debugY = framebufferInfo->ScreenHeight > debugHeight + 16u
        ? (framebufferInfo->ScreenHeight - debugHeight - 16u)
        : 0u;
    ClearRect(framebufferInfo, 24u, debugY, 420u, debugHeight, black);

    writeCursor = line;
    AppendText(&writeCursor, "Raw ");
    AppendInt(&writeCursor, cursor->LastRawX);
    AppendText(&writeCursor, ",");
    AppendInt(&writeCursor, cursor->LastRawY);
    AppendText(&writeCursor, " Poll ");
    AppendInt(&writeCursor, (INT32)cursor->LastPollCount);
    *writeCursor = 0;
    DrawTextString(framebufferInfo, 24, (INT32)debugY, line, 170u, 220u, 255u);

    writeCursor = line;
    AppendText(&writeCursor, "Px ");
    AppendInt(&writeCursor, cursor->LastPixelX);
    AppendText(&writeCursor, ",");
    AppendInt(&writeCursor, cursor->LastPixelY);
    *writeCursor = 0;
    DrawTextString(framebufferInfo, 24, (INT32)(debugY + FONT_GLYPH_HEIGHT), line, 170u, 255u, 170u);
}

static UINT32 ClampCoordinate(INT32 value, UINT32 limit)
{
    INT32 maxValue;

    if (limit == 0u)
    {
        return 0u;
    }

    if (value < 0)
    {
        return 0u;
    }

    maxValue = (INT32)(limit - 1u);
    if (value > maxValue)
    {
        return limit - 1u;
    }

    return (UINT32)value;
}

static UINT32 ClampCursorX(INT32 value, const FRAMEBUFFER_INFO* framebufferInfo)
{
    UINT32 limit = framebufferInfo->ScreenWidth > CURSOR_WIDTH
        ? (framebufferInfo->ScreenWidth - CURSOR_WIDTH)
        : 0u;
    return ClampCoordinate(value, limit + 1u);
}

static UINT32 ClampCursorY(INT32 value, const FRAMEBUFFER_INFO* framebufferInfo)
{
    UINT32 limit = framebufferInfo->ScreenHeight > CURSOR_HEIGHT
        ? (framebufferInfo->ScreenHeight - CURSOR_HEIGHT)
        : 0u;
    return ClampCoordinate(value, limit + 1u);
}

static INT32 ClampI32(INT32 Value, INT32 Min, INT32 Max)
{
    if (Value < Min) return Min;
    if (Value > Max) return Max;
    return Value;
}

static INT32 ApplyMouseCurve(INT32 Delta)
{
    INT32 AbsDelta;
    INT32 Curve;

    AbsDelta = (Delta < 0) ? -Delta : Delta;

    if (AbsDelta <= 1)
        return Delta;

    Curve = AbsDelta + (AbsDelta * AbsDelta) / 8;

    if (Curve > 64)
        Curve = 64;

    return (Delta < 0) ? -Curve : Curve;
}

#define POINTER_SENSITIVITY_PCT  100u
#define FIXED_PIXEL_SHIFT 8
#define FIXED_PIXEL_ONE (1 << FIXED_PIXEL_SHIFT)

static INT32 CountsToFixedPixels(INT32 Counts, UINT64 Resolution, UINT32 Span)
{
    INT64 Pixels;
    UINT64 Res;

    if (Counts == 0 || Span == 0)
        return 0;

    // UEFI reports resolution in counts/mm. Clamp to a sane range —
    // firmware often lies (reports 0 or absurdly large values)
    Res = Resolution;
    if (Res == 0u || Res > 400u)
        Res = 8u;  // ~200dpi mouse, a safe fallback

    Pixels = ((INT64)Counts * (INT64)Span * 2LL * (INT64)POINTER_SENSITIVITY_PCT * (INT64)FIXED_PIXEL_ONE)
        / ((INT64)Res * 10000LL);

    return (INT32)Pixels;
}

static INT32 ApplyMouseAcceleration(INT32 DeltaFixed)
{
    INT32 AbsDelta;
    UINT32 Gain1024;
    INT64 Accelerated;

    AbsDelta = (DeltaFixed < 0) ? -DeltaFixed : DeltaFixed;
    Gain1024 = 1024u + (UINT32)((AbsDelta * 192) / FIXED_PIXEL_ONE);
    if (Gain1024 > 3072u)
    {
        Gain1024 = 3072u;
    }

    Accelerated = ((INT64)DeltaFixed * (INT64)Gain1024) / 1024LL;
    return (INT32)Accelerated;
}

static UINT8 TryUpdateCursorFromAbsolutePointer(
    EFI_ABSOLUTE_POINTER_PROTOCOL* Pointer,
    SOFTWARE_CURSOR* Cursor,
    const FRAMEBUFFER_INFO* FramebufferInfo)
{
    EFI_ABSOLUTE_POINTER_STATE State;
    EFI_STATUS Status;
    UINT8 HasState;
    UINT64 RangeX, RangeY;
    UINT32 NewX, NewY;

    if (Pointer == 0 || Cursor == 0 || FramebufferInfo == 0 || Pointer->Mode == 0)
        return 0u;

    HasState = 0u;
    for (;;)
    {
        Status = Pointer->GetState(Pointer, &State);
        if (Status == EFI_NOT_READY)
        {
            break;
        }

        if (Status != EFI_SUCCESS)
        {
            return 0u;
        }

        HasState = 1u;
    }

    if (HasState == 0u)
    {
        return 0u;
    }

    RangeX = Pointer->Mode->AbsoluteMaxX - Pointer->Mode->AbsoluteMinX;
    RangeY = Pointer->Mode->AbsoluteMaxY - Pointer->Mode->AbsoluteMinY;

    if (RangeX == 0u || RangeY == 0u)
        return 0u;

    NewX = (UINT32)(((State.CurrentX - Pointer->Mode->AbsoluteMinX) *
        (UINT64)FramebufferInfo->ScreenWidth) / RangeX);
    NewY = (UINT32)(((State.CurrentY - Pointer->Mode->AbsoluteMinY) *
        (UINT64)FramebufferInfo->ScreenHeight) / RangeY);

    Cursor->LeftButtonDown = (UINT8)((State.ActiveButtons & 1u) != 0u);

    if (NewX == Cursor->X && NewY == Cursor->Y)
        return 0u;

    Cursor->X = ClampCursorX((INT32)NewX, FramebufferInfo);
    Cursor->Y = ClampCursorY((INT32)NewY, FramebufferInfo);
    return 1u;
}

static UINT8 TryUpdateCursorFromSimplePointer(
    EFI_SIMPLE_POINTER_PROTOCOL* Pointer,
    SOFTWARE_CURSOR* Cursor,
    const FRAMEBUFFER_INFO* FramebufferInfo)
{
    EFI_SIMPLE_POINTER_STATE State;
    EFI_STATUS Status;
    INT64 TotalCountsX;
    INT64 TotalCountsY;
    UINT32 PollCount;
    INT32 DeltaX;
    INT32 DeltaY;
    INT32 DeltaXFixed;
    INT32 DeltaYFixed;
    INT32 NewX;
    INT32 NewY;
    UINT64 ResolutionX;
    UINT64 ResolutionY;
    UINT8 LeftButtonDown;

    if (Pointer == 0 || Cursor == 0 || FramebufferInfo == 0)
        return 0u;

    TotalCountsX = 0;
    TotalCountsY = 0;
    PollCount = 0u;
    LeftButtonDown = Cursor->LeftButtonDown;
    for (;;)
    {
        Status = Pointer->GetState(Pointer, &State);
        if (Status == EFI_NOT_READY)
        {
            break;
        }

        if (Status != EFI_SUCCESS)
        {
            return 0u;
        }

        TotalCountsX += (INT64)State.RelativeMovementX;
        TotalCountsY += (INT64)State.RelativeMovementY;
        LeftButtonDown = State.LeftButton != 0u ? 1u : 0u;
        PollCount++;
        if (PollCount >= 64u)
        {
            break;
        }
    }

    if (PollCount == 0u)
        return 0u;

    Cursor->LeftButtonDown = LeftButtonDown;
    Cursor->LastRawX = ClampI32((INT32)TotalCountsX, -9999, 9999);
    Cursor->LastRawY = ClampI32((INT32)TotalCountsY, -9999, 9999);
    Cursor->LastPollCount = PollCount;

    if (TotalCountsX == 0 && TotalCountsY == 0)
    {
        Cursor->LastPixelX = 0;
        Cursor->LastPixelY = 0;
        return 0u;
    }

    ResolutionX = (Pointer->Mode != 0) ? Pointer->Mode->ResolutionX : 0u;
    ResolutionY = (Pointer->Mode != 0) ? Pointer->Mode->ResolutionY : 0u;

    DeltaXFixed = CountsToFixedPixels(
        ApplyMouseCurve(ClampI32((INT32)TotalCountsX, -256, 256)),
        ResolutionX,
        FramebufferInfo->ScreenWidth);
    DeltaYFixed = CountsToFixedPixels(
        ApplyMouseCurve(ClampI32((INT32)TotalCountsY, -256, 256)),
        ResolutionY,
        FramebufferInfo->ScreenHeight);

    if (DeltaXFixed == 0 && TotalCountsX != 0)
    {
        DeltaXFixed = ClampI32((INT32)TotalCountsX, -8, 8) * (FIXED_PIXEL_ONE / 2);
    }

    if (DeltaYFixed == 0 && TotalCountsY != 0)
    {
        DeltaYFixed = ClampI32((INT32)TotalCountsY, -8, 8) * (FIXED_PIXEL_ONE / 2);
    }

    DeltaXFixed = ApplyMouseAcceleration(DeltaXFixed);
    DeltaYFixed = ApplyMouseAcceleration(DeltaYFixed);

    Cursor->AccumX = ClampI32(Cursor->AccumX + DeltaXFixed, -(FIXED_PIXEL_ONE * 64), FIXED_PIXEL_ONE * 64);
    Cursor->AccumY = ClampI32(Cursor->AccumY + DeltaYFixed, -(FIXED_PIXEL_ONE * 64), FIXED_PIXEL_ONE * 64);

    DeltaX = Cursor->AccumX / FIXED_PIXEL_ONE;
    DeltaY = Cursor->AccumY / FIXED_PIXEL_ONE;
    Cursor->AccumX -= DeltaX * FIXED_PIXEL_ONE;
    Cursor->AccumY -= DeltaY * FIXED_PIXEL_ONE;
    Cursor->LastPixelX = DeltaX;
    Cursor->LastPixelY = DeltaY;

    if (DeltaX == 0 && DeltaY == 0)
    {
        return 0u;
    }

    NewX = (INT32)Cursor->X + DeltaX;
    NewY = (INT32)Cursor->Y + DeltaY;

    Cursor->X = ClampCursorX(NewX, FramebufferInfo);
    Cursor->Y = ClampCursorY(NewY, FramebufferInfo);
    return 1u;
}

static void BusyPause(UINT32 iterations)
{
    volatile UINT32 index;

    for (index = 0u; index < iterations; index++)
    {
    }
}

static UINT8 Ps2WaitForInputClear(void)
{
    UINT32 attempt;

    for (attempt = 0u; attempt < 100000u; attempt++)
    {
        if ((__inbyte(0x64u) & 0x02u) == 0u)
        {
            return 1u;
        }

        BusyPause(32u);
    }

    return 0u;
}

static UINT8 Ps2WaitForOutputByte(UINT8 requireMouseData, UINT8* dataOut)
{
    UINT32 attempt;

    for (attempt = 0u; attempt < 100000u; attempt++)
    {
        UINT8 status = __inbyte(0x64u);
        if ((status & 0x01u) != 0u)
        {
            UINT8 data = __inbyte(0x60u);
            if (requireMouseData == 0u || (status & 0x20u) != 0u)
            {
                if (dataOut != 0)
                {
                    *dataOut = data;
                }

                return 1u;
            }
        }

        BusyPause(32u);
    }

    return 0u;
}

static UINT8 Ps2TryReadByte(UINT8* dataOut, UINT8* isMouseDataOut)
{
    UINT8 status;

    status = __inbyte(0x64u);
    if ((status & 0x01u) == 0u)
    {
        return 0u;
    }

    if (dataOut != 0)
    {
        *dataOut = __inbyte(0x60u);
    }
    else
    {
        (void)__inbyte(0x60u);
    }

    if (isMouseDataOut != 0)
    {
        *isMouseDataOut = (UINT8)(((status & 0x20u) != 0u) ? 1u : 0u);
    }

    return 1u;
}

static void Ps2FlushOutputBuffer(void)
{
    UINT32 flushCount;

    for (flushCount = 0u; flushCount < 64u; flushCount++)
    {
        if ((__inbyte(0x64u) & 0x01u) == 0u)
        {
            break;
        }

        (void)__inbyte(0x60u);
        BusyPause(16u);
    }
}

static UINT8 Ps2WriteControllerCommand(UINT8 command)
{
    if (Ps2WaitForInputClear() == 0u)
    {
        return 0u;
    }

    __outbyte(0x64u, command);
    return 1u;
}

static UINT8 Ps2WriteControllerData(UINT8 value)
{
    if (Ps2WaitForInputClear() == 0u)
    {
        return 0u;
    }

    __outbyte(0x60u, value);
    return 1u;
}

static UINT8 Ps2WriteMouseCommand(UINT8 command)
{
    if (Ps2WriteControllerCommand(0xD4u) == 0u)
    {
        return 0u;
    }

    if (Ps2WriteControllerData(command) == 0u)
    {
        return 0u;
    }

    return 1u;
}

static UINT8 InitializePs2Mouse(PS2_MOUSE_STATE* mouseState)
{
    UINT8 controllerConfig;
    UINT8 acknowledge;

    if (mouseState == 0)
    {
        return 0u;
    }

    mouseState->Enabled = 0u;
    mouseState->PacketIndex = 0u;
    mouseState->LeftButtonDown = 0u;
    mouseState->Reserved = 0u;
    mouseState->LastDeltaX = 0;
    mouseState->LastDeltaY = 0;
    mouseState->PacketCount = 0u;

    Ps2FlushOutputBuffer();

    if (Ps2WriteControllerCommand(0xA8u) == 0u)
    {
        return 0u;
    }

    if (Ps2WriteControllerCommand(0x20u) == 0u)
    {
        return 0u;
    }

    if (Ps2WaitForOutputByte(0u, &controllerConfig) == 0u)
    {
        return 0u;
    }

    controllerConfig = (UINT8)(controllerConfig | 0x02u);
    controllerConfig = (UINT8)(controllerConfig & 0xDFu);

    if (Ps2WriteControllerCommand(0x60u) == 0u)
    {
        return 0u;
    }

    if (Ps2WriteControllerData(controllerConfig) == 0u)
    {
        return 0u;
    }

    Ps2FlushOutputBuffer();

    if (Ps2WriteMouseCommand(0xF6u) == 0u)
    {
        return 0u;
    }

    if (Ps2WaitForOutputByte(1u, &acknowledge) == 0u || acknowledge != 0xFAu)
    {
        return 0u;
    }

    if (Ps2WriteMouseCommand(0xF4u) == 0u)
    {
        return 0u;
    }

    if (Ps2WaitForOutputByte(1u, &acknowledge) == 0u || acknowledge != 0xFAu)
    {
        return 0u;
    }

    mouseState->Enabled = 1u;
    return 1u;
}

static UINT8 UpdateCursorFromPs2Mouse(
    PS2_MOUSE_STATE* mouseState,
    SOFTWARE_CURSOR* cursor,
    const FRAMEBUFFER_INFO* framebufferInfo)
{
    UINT32 readCount;
    INT32 totalDeltaX;
    INT32 totalDeltaY;
    UINT32 packetCount;
    UINT8 leftButtonDown;

    if (mouseState == 0 || cursor == 0 || framebufferInfo == 0 || mouseState->Enabled == 0u)
    {
        return 0u;
    }

    readCount = 0u;
    totalDeltaX = 0;
    totalDeltaY = 0;
    packetCount = 0u;
    leftButtonDown = cursor->LeftButtonDown;

    while (readCount < 96u)
    {
        UINT8 data;
        UINT8 isMouseData;

        if (Ps2TryReadByte(&data, &isMouseData) == 0u)
        {
            break;
        }

        readCount++;
        if (isMouseData == 0u)
        {
            continue;
        }

        if (mouseState->PacketIndex == 0u && (data & 0x08u) == 0u)
        {
            continue;
        }

        mouseState->Packet[mouseState->PacketIndex++] = data;
        if (mouseState->PacketIndex < 3u)
        {
            continue;
        }

        mouseState->PacketIndex = 0u;
        leftButtonDown = (UINT8)((mouseState->Packet[0] & 0x01u) != 0u ? 1u : 0u);
        if ((mouseState->Packet[0] & 0xC0u) != 0u)
        {
            continue;
        }

        totalDeltaX += (INT32)(INT8)mouseState->Packet[1];
        totalDeltaY += (INT32)(INT8)mouseState->Packet[2];
        packetCount++;
        if (packetCount >= 32u)
        {
            break;
        }
    }

    cursor->LeftButtonDown = leftButtonDown;
    cursor->LastPollCount = packetCount;
    if (packetCount == 0u)
    {
        return 0u;
    }

    cursor->LastRawX = ClampI32(totalDeltaX, -9999, 9999);
    cursor->LastRawY = ClampI32(totalDeltaY, -9999, 9999);

    if (totalDeltaX == 0 && totalDeltaY == 0)
    {
        cursor->LastPixelX = 0;
        cursor->LastPixelY = 0;
        return 1u;
    }

    mouseState->LastDeltaX = ApplyMouseAcceleration(ClampI32(totalDeltaX, -64, 64) * (FIXED_PIXEL_ONE / 2));
    mouseState->LastDeltaY = ApplyMouseAcceleration(ClampI32(-totalDeltaY, -64, 64) * (FIXED_PIXEL_ONE / 2));

    cursor->AccumX = ClampI32(cursor->AccumX + mouseState->LastDeltaX, -(FIXED_PIXEL_ONE * 64), FIXED_PIXEL_ONE * 64);
    cursor->AccumY = ClampI32(cursor->AccumY + mouseState->LastDeltaY, -(FIXED_PIXEL_ONE * 64), FIXED_PIXEL_ONE * 64);

    cursor->LastPixelX = cursor->AccumX / FIXED_PIXEL_ONE;
    cursor->LastPixelY = cursor->AccumY / FIXED_PIXEL_ONE;
    cursor->AccumX -= cursor->LastPixelX * FIXED_PIXEL_ONE;
    cursor->AccumY -= cursor->LastPixelY * FIXED_PIXEL_ONE;

    if (cursor->LastPixelX != 0 || cursor->LastPixelY != 0)
    {
        cursor->X = ClampCursorX((INT32)cursor->X + cursor->LastPixelX, framebufferInfo);
        cursor->Y = ClampCursorY((INT32)cursor->Y + cursor->LastPixelY, framebufferInfo);
    }

    mouseState->PacketCount += packetCount;
    return 1u;
}

static void InitializeSetupUiState(const FRAMEBUFFER_INFO* framebufferInfo, SETUP_UI_STATE* setupUiState, UINT8 mouseDetected)
{
    UINT32 panelMarginX;
    UINT32 panelMarginY;
    UINT32 buttonWidth;
    UINT32 buttonHeight;
    UINT32 buttonX;
    UINT32 buttonGap;
    UINT32 buttonStartY;

    if (framebufferInfo == 0 || setupUiState == 0)
    {
        return;
    }

    panelMarginX = framebufferInfo->ScreenWidth / 16u;
    if (panelMarginX < 48u)
    {
        panelMarginX = 48u;
    }

    panelMarginY = framebufferInfo->ScreenHeight / 14u;
    if (panelMarginY < 44u)
    {
        panelMarginY = 44u;
    }

    setupUiState->Panel.X = panelMarginX;
    setupUiState->Panel.Y = panelMarginY;
    setupUiState->Panel.Width = framebufferInfo->ScreenWidth - (panelMarginX * 2u);
    setupUiState->Panel.Height = framebufferInfo->ScreenHeight - (panelMarginY * 2u);
    setupUiState->Panel.Radius = 28u;
    setupUiState->Panel.Color = ComposePixel(250u, 250u, 250u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);

    buttonWidth = setupUiState->Panel.Width / 4u;
    if (buttonWidth < 240u)
    {
        buttonWidth = 240u;
    }

    buttonHeight = 64u;
    buttonGap = 34u;
    buttonX = setupUiState->Panel.X + setupUiState->Panel.Width - buttonWidth - 72u;
    buttonStartY = setupUiState->Panel.Y + setupUiState->Panel.Height - (buttonHeight * 2u) - buttonGap - 120u;

    setupUiState->InstallButton.X = buttonX;
    setupUiState->InstallButton.Y = buttonStartY;
    setupUiState->InstallButton.Width = buttonWidth;
    setupUiState->InstallButton.Height = buttonHeight;
    setupUiState->InstallButton.Radius = 24u;
    setupUiState->InstallButton.Color = ComposePixel(227u, 227u, 229u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);

    setupUiState->TryButton = setupUiState->InstallButton;
    setupUiState->TryButton.Y = buttonStartY + buttonHeight + buttonGap;
    setupUiState->HoveredButton = 0u;
    setupUiState->MouseDetected = mouseDetected;
}

static UINT32 HitTestSetupUi(const SETUP_UI_STATE* setupUiState, const SOFTWARE_CURSOR* cursor)
{
    UINT32 centerX;
    UINT32 centerY;

    if (setupUiState == 0 || cursor == 0)
    {
        return 0u;
    }

    centerX = cursor->X + (CURSOR_WIDTH / 2u);
    centerY = cursor->Y + (CURSOR_HEIGHT / 2u);

    if (PointInRoundedBox(centerX, centerY, &setupUiState->InstallButton) != 0u)
    {
        return 1u;
    }

    if (PointInRoundedBox(centerX, centerY, &setupUiState->TryButton) != 0u)
    {
        return 2u;
    }

    return 0u;
}

static void DrawSetupButton(const FRAMEBUFFER_INFO* framebufferInfo, const UI_BOX* button, const char* label, UINT8 hovered)
{
    UI_BOX buttonBox;
    UINT32 buttonColor;
    INT32 buttonTextX;
    INT32 buttonTextY;

    if (framebufferInfo == 0 || button == 0 || label == 0)
    {
        return;
    }

    buttonColor = hovered != 0u
        ? ComposePixel(212u, 212u, 215u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask)
        : ComposePixel(227u, 227u, 229u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);
    buttonBox = *button;
    buttonBox.Color = buttonColor;
    DrawRoundedBox(framebufferInfo, &buttonBox);

    buttonTextX = (INT32)button->X + ((INT32)button->Width - MeasureTextStringAtlas(label, SETUP_UI_FONT_FIRST_CHAR, SETUP_UI_FONT_GLYPH_COUNT, gSetupUiFontGlyphAdvance, SETUP_UI_FONT_GLYPH_WIDTH / 2u)) / 2;
    buttonTextY = (INT32)button->Y + (((INT32)button->Height - (INT32)SETUP_UI_FONT_GLYPH_HEIGHT) / 2);
    DrawTextStringAtlas(framebufferInfo, buttonTextX, buttonTextY, label, 12u, 12u, 12u, SETUP_UI_FONT_FIRST_CHAR, SETUP_UI_FONT_GLYPH_COUNT, SETUP_UI_FONT_GLYPH_WIDTH, SETUP_UI_FONT_GLYPH_HEIGHT, gSetupUiFontGlyphAlpha, gSetupUiFontGlyphAdvance);
}

static void RefreshSetupButtons(const FRAMEBUFFER_INFO* framebufferInfo, const SETUP_UI_STATE* setupUiState, UINT32 previousHoveredButton, UINT32 currentHoveredButton)
{
    if (framebufferInfo == 0 || setupUiState == 0 || previousHoveredButton == currentHoveredButton)
    {
        return;
    }

    if (previousHoveredButton == 1u || currentHoveredButton == 1u)
    {
        DrawSetupButton(framebufferInfo, &setupUiState->InstallButton, "Install", currentHoveredButton == 1u ? 1u : 0u);
    }

    if (previousHoveredButton == 2u || currentHoveredButton == 2u)
    {
        DrawSetupButton(framebufferInfo, &setupUiState->TryButton, "Try", currentHoveredButton == 2u ? 1u : 0u);
    }
}

static void DrawSetupScreen(const FRAMEBUFFER_INFO* framebufferInfo, const SETUP_UI_STATE* setupUiState)
{
    static const char* titleText = "Dragon OS Setup";
    static const char* promptText = "What would you like to do with Dragon OS?";
    UINT32 backgroundColor;
    UINT32 iconDrawHeight;
    UINT32 iconDrawWidth;
    INT32 titleX;
    INT32 promptX;
    INT32 titleY;
    INT32 promptY;
    INT32 rightZoneX;
    INT32 rightZoneWidth;

    if (framebufferInfo == 0 || setupUiState == 0)
    {
        return;
    }

    backgroundColor = ComposePixel(208u, 208u, 208u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);

    ClearScreen(framebufferInfo, backgroundColor, 0u);
    DrawRoundedBox(framebufferInfo, &setupUiState->Panel);

    titleY = (INT32)setupUiState->Panel.Y + 48;
    titleX = (INT32)setupUiState->Panel.X + ((INT32)setupUiState->Panel.Width - MeasureTextStringAtlas(titleText, SETUP_TITLE_FONT_FIRST_CHAR, SETUP_TITLE_FONT_GLYPH_COUNT, gSetupTitleFontGlyphAdvance, SETUP_TITLE_FONT_GLYPH_WIDTH / 2u)) / 2;
    DrawGradientOutlinedTextAtlas(framebufferInfo, titleX, titleY, titleText, 101u, 235u, 255u, 76u, 255u, 84u, 3u, SETUP_TITLE_FONT_FIRST_CHAR, SETUP_TITLE_FONT_GLYPH_COUNT, SETUP_TITLE_FONT_GLYPH_WIDTH, SETUP_TITLE_FONT_GLYPH_HEIGHT, gSetupTitleFontGlyphAlpha, gSetupTitleFontGlyphAdvance, SETUP_TITLE_FONT_GLYPH_WIDTH / 2u);

    iconDrawHeight = setupUiState->Panel.Height > 280u ? (setupUiState->Panel.Height - 250u) : (setupUiState->Panel.Height / 2u);
    if (iconDrawHeight > 420u)
    {
        iconDrawHeight = 420u;
    }

    if (iconDrawHeight < 200u)
    {
        iconDrawHeight = 200u;
    }

    iconDrawWidth = (SETUP_ICON_WIDTH * iconDrawHeight) / SETUP_ICON_HEIGHT;
    DrawImageNearestAlpha(
        framebufferInfo,
        (INT32)setupUiState->Panel.X + 58,
        (INT32)setupUiState->Panel.Y + 160,
        iconDrawWidth,
        iconDrawHeight,
        SETUP_ICON_WIDTH,
        SETUP_ICON_HEIGHT,
        gSetupIconPixels,
        gSetupIconAlpha);

    rightZoneX = (INT32)setupUiState->Panel.X + (INT32)(setupUiState->Panel.Width / 2u) - 10;
    rightZoneWidth = (INT32)setupUiState->Panel.Width - (rightZoneX - (INT32)setupUiState->Panel.X) - 48;
    promptY = (INT32)setupUiState->Panel.Y + 210;
    promptX = rightZoneX + (rightZoneWidth - MeasureTextStringAtlas(promptText, SETUP_UI_FONT_FIRST_CHAR, SETUP_UI_FONT_GLYPH_COUNT, gSetupUiFontGlyphAdvance, SETUP_UI_FONT_GLYPH_WIDTH / 2u)) / 2;
    DrawTextStringAtlas(framebufferInfo, promptX, promptY, promptText, 18u, 18u, 18u, SETUP_UI_FONT_FIRST_CHAR, SETUP_UI_FONT_GLYPH_COUNT, SETUP_UI_FONT_GLYPH_WIDTH, SETUP_UI_FONT_GLYPH_HEIGHT, gSetupUiFontGlyphAlpha, gSetupUiFontGlyphAdvance);

    DrawSetupButton(framebufferInfo, &setupUiState->InstallButton, "Install", setupUiState->HoveredButton == 1u ? 1u : 0u);
    DrawSetupButton(framebufferInfo, &setupUiState->TryButton, "Try", setupUiState->HoveredButton == 2u ? 1u : 0u);

    if (setupUiState->MouseDetected == 0u)
    {
        DrawTextString(framebufferInfo, (INT32)setupUiState->Panel.X + 34, (INT32)(setupUiState->Panel.Y + setupUiState->Panel.Height - 34), "USB combo mouse still needs a future HID driver on real hardware.", 128u, 128u, 128u);
    }
}

static void GetTweenDemoDynamicRect(const FRAMEBUFFER_INFO* framebufferInfo, UINT32* xOut, UINT32* yOut, UINT32* widthOut, UINT32* heightOut)
{
    UINT32 panelWidth;
    UINT32 panelHeight;
    UINT32 panelX;
    UINT32 panelY;

    panelWidth = framebufferInfo->ScreenWidth - (framebufferInfo->ScreenWidth / 8u);
    panelHeight = framebufferInfo->ScreenHeight - (framebufferInfo->ScreenHeight / 6u);
    panelX = (framebufferInfo->ScreenWidth - panelWidth) / 2u;
    panelY = (framebufferInfo->ScreenHeight - panelHeight) / 2u;

    if (xOut != 0)
    {
        *xOut = panelX + 48u;
    }

    if (yOut != 0)
    {
        *yOut = panelY + 112u;
    }

    if (widthOut != 0)
    {
        *widthOut = panelWidth - 96u;
    }

    if (heightOut != 0)
    {
        *heightOut = panelHeight - 148u;
    }
}

static void DrawTweenDemoBase(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 tweenFamily)
{
    static const char* titleText = "Dragon OS Tween Test";
    UINT32 backgroundColor;
    UINT32 panelColor;
    UINT32 panelWidth;
    UINT32 panelHeight;
    UINT32 panelX;
    UINT32 panelY;
    INT32 titleX;
    INT32 subtitleX;
    UI_BOX panel;

    if (framebufferInfo == 0)
    {
        return;
    }

    (void)tweenFamily;

    backgroundColor = ComposePixel(18u, 18u, 22u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);
    panelColor = ComposePixel(245u, 245u, 247u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);

    ClearScreen(framebufferInfo, backgroundColor, 0u);

    panelWidth = framebufferInfo->ScreenWidth - (framebufferInfo->ScreenWidth / 8u);
    panelHeight = framebufferInfo->ScreenHeight - (framebufferInfo->ScreenHeight / 6u);
    panelX = (framebufferInfo->ScreenWidth - panelWidth) / 2u;
    panelY = (framebufferInfo->ScreenHeight - panelHeight) / 2u;

    panel.X = panelX;
    panel.Y = panelY;
    panel.Width = panelWidth;
    panel.Height = panelHeight;
    panel.Radius = 28u;
    panel.Color = panelColor;
    panel.Dragging = 0u;
    panel.DragOffsetX = 0;
    panel.DragOffsetY = 0;
    DrawRoundedBox(framebufferInfo, &panel);

    titleX = (INT32)panelX + ((INT32)panelWidth - MeasureTextStringAtlas(titleText, SETUP_TITLE_FONT_FIRST_CHAR, SETUP_TITLE_FONT_GLYPH_COUNT, gSetupTitleFontGlyphAdvance, SETUP_TITLE_FONT_GLYPH_WIDTH / 2u)) / 2;
    DrawGradientOutlinedTextAtlas(framebufferInfo, titleX, (INT32)panelY + 42, titleText, 101u, 235u, 255u, 76u, 255u, 84u, 3u, SETUP_TITLE_FONT_FIRST_CHAR, SETUP_TITLE_FONT_GLYPH_COUNT, SETUP_TITLE_FONT_GLYPH_WIDTH, SETUP_TITLE_FONT_GLYPH_HEIGHT, gSetupTitleFontGlyphAlpha, gSetupTitleFontGlyphAdvance, SETUP_TITLE_FONT_GLYPH_WIDTH / 2u);

    subtitleX = (INT32)panelX;
    (void)subtitleX;
}

static void DrawTweenDemoMotion(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 tweenFamily, UINT32 progress1024)
{
    static const char* familyName = "Linear";
    static const char* modeName = "InOut";
    char tweenLabel[64];
    char progressLabel[48];
    char* cursor;
    UINT32 panelColor;
    UINT32 boxColor;
    UINT8 boxRed;
    UINT8 boxGreen;
    UINT8 boxBlue;
    UINT32 eased1024;
    UINT32 panelWidth;
    UINT32 panelHeight;
    UINT32 panelX;
    UINT32 panelY;
    UINT32 boxWidth;
    UINT32 boxHeight;
    UINT32 minX;
    UINT32 travelWidth;
    UINT32 boxX;
    UINT32 boxY;
    UINT32 dynamicX;
    UINT32 dynamicY;
    UINT32 dynamicWidth;
    UINT32 dynamicHeight;
    INT32 progressX;
    UI_BOX tweenBox;

    if (framebufferInfo == 0)
    {
        return;
    }

    if (tweenFamily == 1u)
    {
        familyName = "Sine";
    }
    else if (tweenFamily == 2u)
    {
        familyName = "Cubic";
    }

    panelWidth = framebufferInfo->ScreenWidth - (framebufferInfo->ScreenWidth / 8u);
    panelHeight = framebufferInfo->ScreenHeight - (framebufferInfo->ScreenHeight / 6u);
    panelX = (framebufferInfo->ScreenWidth - panelWidth) / 2u;
    panelY = (framebufferInfo->ScreenHeight - panelHeight) / 2u;
    panelColor = ComposePixel(245u, 245u, 247u, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);
    eased1024 = EvaluateTween(tweenFamily, 2u, progress1024);
    boxRed = (UINT8)(101u + (((INT32)76 - 101) * (INT32)eased1024) / 1024);
    boxGreen = (UINT8)(235u + (((INT32)255 - 235) * (INT32)eased1024) / 1024);
    boxBlue = (UINT8)(255u + (((INT32)84 - 255) * (INT32)eased1024) / 1024);
    boxColor = ComposePixel(boxRed, boxGreen, boxBlue, framebufferInfo->PixelFormat, framebufferInfo->RedMask, framebufferInfo->GreenMask, framebufferInfo->BlueMask);

    GetTweenDemoDynamicRect(framebufferInfo, &dynamicX, &dynamicY, &dynamicWidth, &dynamicHeight);
    ClearRect(framebufferInfo, dynamicX, dynamicY, dynamicWidth, dynamicHeight, panelColor);

    cursor = tweenLabel;
    AppendText(&cursor, "Current Tween: ");
    AppendText(&cursor, familyName);
    AppendText(&cursor, " ");
    AppendText(&cursor, modeName);
    *cursor = 0;

    cursor = progressLabel;
    AppendText(&cursor, "Progress ");
    AppendInt(&cursor, (INT32)((progress1024 * 100u) / 1024u));
    AppendText(&cursor, "%");
    *cursor = 0;

    progressX = (INT32)panelX + ((INT32)panelWidth - MeasureTextStringAtlas(tweenLabel, SETUP_UI_FONT_FIRST_CHAR, SETUP_UI_FONT_GLYPH_COUNT, gSetupUiFontGlyphAdvance, SETUP_UI_FONT_GLYPH_WIDTH / 2u)) / 2;
    DrawTextStringAtlas(framebufferInfo, progressX, (INT32)panelY + 132, tweenLabel, 22u, 22u, 28u, SETUP_UI_FONT_FIRST_CHAR, SETUP_UI_FONT_GLYPH_COUNT, SETUP_UI_FONT_GLYPH_WIDTH, SETUP_UI_FONT_GLYPH_HEIGHT, gSetupUiFontGlyphAlpha, gSetupUiFontGlyphAdvance);

    progressX = (INT32)panelX + ((INT32)panelWidth - MeasureTextStringAtlas(progressLabel, SETUP_UI_FONT_FIRST_CHAR, SETUP_UI_FONT_GLYPH_COUNT, gSetupUiFontGlyphAdvance, SETUP_UI_FONT_GLYPH_WIDTH / 2u)) / 2;
    DrawTextStringAtlas(framebufferInfo, progressX, (INT32)panelY + 168, progressLabel, 72u, 72u, 80u, SETUP_UI_FONT_FIRST_CHAR, SETUP_UI_FONT_GLYPH_COUNT, SETUP_UI_FONT_GLYPH_WIDTH, SETUP_UI_FONT_GLYPH_HEIGHT, gSetupUiFontGlyphAlpha, gSetupUiFontGlyphAdvance);

    boxWidth = panelWidth / 5u;
    if (boxWidth < 180u)
    {
        boxWidth = 180u;
    }

    boxHeight = 72u;
    minX = panelX + 72u;
    travelWidth = panelWidth - boxWidth - 144u;
    boxX = minX + ((travelWidth * eased1024) / 1024u);
    boxY = panelY + (panelHeight / 2u);

    tweenBox.X = boxX;
    tweenBox.Y = boxY;
    tweenBox.Width = boxWidth;
    tweenBox.Height = boxHeight;
    tweenBox.Radius = 26u;
    tweenBox.Color = boxColor;
    tweenBox.Dragging = 0u;
    tweenBox.DragOffsetX = 0;
    tweenBox.DragOffsetY = 0;
    DrawRoundedBox(framebufferInfo, &tweenBox);

    DrawTextStringAtlas(
        framebufferInfo,
        (INT32)boxX + ((INT32)boxWidth - MeasureTextStringAtlas(familyName, SETUP_UI_FONT_FIRST_CHAR, SETUP_UI_FONT_GLYPH_COUNT, gSetupUiFontGlyphAdvance, SETUP_UI_FONT_GLYPH_WIDTH / 2u)) / 2,
        (INT32)boxY + (((INT32)boxHeight - (INT32)SETUP_UI_FONT_GLYPH_HEIGHT) / 2),
        familyName,
        12u,
        12u,
        12u,
        SETUP_UI_FONT_FIRST_CHAR,
        SETUP_UI_FONT_GLYPH_COUNT,
        SETUP_UI_FONT_GLYPH_WIDTH,
        SETUP_UI_FONT_GLYPH_HEIGHT,
        gSetupUiFontGlyphAlpha,
        gSetupUiFontGlyphAdvance);
}

static void DrawTweenDemoFrame(const FRAMEBUFFER_INFO* framebufferInfo, UINT32 tweenFamily, UINT32 progress1024)
{
    DrawTweenDemoBase(framebufferInfo, tweenFamily);
    DrawTweenDemoMotion(framebufferInfo, tweenFamily, progress1024);
}

static UINT64 EstimateTscTicksPerSecond(EFI_STALL stall)
{
    UINT64 startTicks;
    UINT64 endTicks;
    UINT64 deltaTicks;

    if (stall == 0)
    {
        return 0u;
    }

    startTicks = __rdtsc();
    (void)stall(50000u);
    endTicks = __rdtsc();
    deltaTicks = endTicks - startTicks;
    if (deltaTicks == 0u)
    {
        return 0u;
    }

    return deltaTicks * 20u;
}

static void RunKernelMouseLoop(const KERNEL_BOOT_INFO* bootInfo)
{
    FRAMEBUFFER_INFO framebufferInfo;
    FRAMEBUFFER_INFO shadowFramebufferInfo;
    FRAMEBUFFER_INFO composeFramebufferInfo;
    SOFTWARE_CURSOR cursor;
    PS2_MOUSE_STATE mouseState;
    SETUP_UI_STATE setupUiState;
    UINT8 mouseDetected;

    if (bootInfo == 0)
    {
        return;
    }

    framebufferInfo = bootInfo->Framebuffer;
    shadowFramebufferInfo = framebufferInfo;
    composeFramebufferInfo = framebufferInfo;
    if (bootInfo->ShadowFramebuffer != 0 && bootInfo->ShadowFramebuffer != framebufferInfo.Framebuffer)
    {
        shadowFramebufferInfo.Framebuffer = bootInfo->ShadowFramebuffer;
    }
    if (bootInfo->ComposeFramebuffer != 0 &&
        bootInfo->ComposeFramebuffer != framebufferInfo.Framebuffer &&
        bootInfo->ComposeFramebuffer != shadowFramebufferInfo.Framebuffer)
    {
        composeFramebufferInfo.Framebuffer = bootInfo->ComposeFramebuffer;
    }
    else
    {
        composeFramebufferInfo.Framebuffer = 0;
    }

    cursor.X = ClampCursorX((INT32)(framebufferInfo.ScreenWidth / 2u), &framebufferInfo);
    cursor.Y = ClampCursorY((INT32)(framebufferInfo.ScreenHeight / 2u), &framebufferInfo);
    cursor.SavedWidth = 0u;
    cursor.SavedHeight = 0u;
    cursor.Visible = 0u;
    cursor.AccumX = 0;
    cursor.AccumY = 0;
    cursor.LeftButtonDown = 0u;
    cursor.LastRawX = 0;
    cursor.LastRawY = 0;
    cursor.LastPixelX = 0;
    cursor.LastPixelY = 0;
    cursor.LastPollCount = 0u;

    InitializeCursorSurface(&framebufferInfo, &cursor);
    mouseDetected = InitializePs2Mouse(&mouseState);
    InitializeSetupUiState(&framebufferInfo, &setupUiState, mouseDetected);
    DrawSetupScreen(&shadowFramebufferInfo, &setupUiState);
    if (shadowFramebufferInfo.Framebuffer != framebufferInfo.Framebuffer)
    {
        CopyPixels32(
            framebufferInfo.Framebuffer,
            shadowFramebufferInfo.Framebuffer,
            framebufferInfo.PixelsPerScanLine * framebufferInfo.ScreenHeight);
    }

    BlitCursorComposite(&framebufferInfo, &shadowFramebufferInfo, &composeFramebufferInfo, &cursor);

    for (;;)
    {
        UINT32 oldCursorX = cursor.X;
        UINT32 oldCursorY = cursor.Y;
        UINT8 oldCursorVisible = cursor.Visible;

        if (UpdateCursorFromPs2Mouse(&mouseState, &cursor, &framebufferInfo) != 0u)
        {
            UINT32 currentX = cursor.X;
            UINT32 currentY = cursor.Y;
            UINT32 hoveredButton;

            if (oldCursorVisible != 0u)
            {
                cursor.X = oldCursorX;
                cursor.Y = oldCursorY;
                BlitSurfaceRect(&framebufferInfo, &shadowFramebufferInfo, cursor.X, cursor.Y, cursor.SavedWidth, cursor.SavedHeight);
                cursor.X = currentX;
                cursor.Y = currentY;
                cursor.Visible = 0u;
            }

            hoveredButton = HitTestSetupUi(&setupUiState, &cursor);
            if (hoveredButton != setupUiState.HoveredButton)
            {
                UINT32 previousHoveredButton = setupUiState.HoveredButton;
                setupUiState.HoveredButton = hoveredButton;
                RefreshSetupButtons(&shadowFramebufferInfo, &setupUiState, previousHoveredButton, hoveredButton);
                if (shadowFramebufferInfo.Framebuffer != framebufferInfo.Framebuffer)
                {
                    if (previousHoveredButton == 1u || hoveredButton == 1u)
                    {
                        BlitSurfaceRect(
                            &framebufferInfo,
                            &shadowFramebufferInfo,
                            setupUiState.InstallButton.X,
                            setupUiState.InstallButton.Y,
                            setupUiState.InstallButton.Width,
                            setupUiState.InstallButton.Height);
                    }

                    if (previousHoveredButton == 2u || hoveredButton == 2u)
                    {
                        BlitSurfaceRect(
                            &framebufferInfo,
                            &shadowFramebufferInfo,
                            setupUiState.TryButton.X,
                            setupUiState.TryButton.Y,
                            setupUiState.TryButton.Width,
                            setupUiState.TryButton.Height);
                    }
                }
            }

            BlitCursorComposite(&framebufferInfo, &shadowFramebufferInfo, &composeFramebufferInfo, &cursor);
        }
        else
        {
            BusyPause(2048u);
        }
    }
}

static void PreserveUiSubsystemReferences(void)
{
    (void)&gSimplePointerProtocolGuid;
    (void)&gAbsolutePointerProtocolGuid;
    (void)&InitializeCursorSurface;
    (void)&RestoreCursorBackground;
    (void)&SaveCursorBackground;
    (void)&DrawCursor;
    (void)&ClearRect;
    (void)&DrawRoundedBox;
    (void)&PointInRoundedBox;
    (void)&DrawMouseDebug;
    (void)&ClampCoordinate;
    (void)&ClampCursorX;
    (void)&ClampCursorY;
    (void)&CountsToFixedPixels;
    (void)&ApplyMouseAcceleration;
    (void)&ClampI32;
    (void)&TryUpdateCursorFromSimplePointer;
    (void)&TryUpdateCursorFromAbsolutePointer;
    (void)&DrawKernelStatusScreen;
}

static void CollectPciDevices(EFI_LOCATE_HANDLE_BUFFER LocateHandleBuffer, EFI_HANDLE_PROTOCOL HandleProtocol, EFI_FREE_POOL FreePool, KERNEL_BOOT_INFO* BootInfo)
{
    EFI_HANDLE* Handles;
    UINTN HandleCount;
    EFI_STATUS Status;
    UINTN HandleIndex;

    Handles = 0;
    HandleCount = 0u;
    Status = LocateHandleBuffer(EFI_BY_PROTOCOL, &gPciIoProtocolGuid, 0, &HandleCount, &Handles);
    if (Status != EFI_SUCCESS || Handles == 0)
    {
        return;
    }

    for (HandleIndex = 0u; HandleIndex < HandleCount; HandleIndex++)
    {
        EFI_PCI_IO_PROTOCOL* PciIo;
        UINT32 IdRegister;
        UINT32 ClassRegister;
        UINT16 VendorId;
        UINT16 DeviceId;
        UINT8 ClassCode;
        UINT8 Subclass;
        UINT8 ProgIf;
        UINT8 Kind;
        UINTN Segment;
        UINTN Bus;
        UINTN Device;
        UINTN Function;

        PciIo = 0;
        Status = HandleProtocol(Handles[HandleIndex], &gPciIoProtocolGuid, (void**)&PciIo);
        if (Status != EFI_SUCCESS || PciIo == 0 || PciIo->Pci.Read == 0)
        {
            continue;
        }

        IdRegister = 0xFFFFFFFFu;
        ClassRegister = 0u;
        if (PciIo->Pci.Read(PciIo, EFI_PCI_IO_PROTOCOL_WIDTH_UINT32, 0u, 1u, &IdRegister) != EFI_SUCCESS ||
            PciIo->Pci.Read(PciIo, EFI_PCI_IO_PROTOCOL_WIDTH_UINT32, 8u, 1u, &ClassRegister) != EFI_SUCCESS)
        {
            continue;
        }

        VendorId = (UINT16)(IdRegister & 0xFFFFu);
        DeviceId = (UINT16)((IdRegister >> 16) & 0xFFFFu);
        if (VendorId == 0xFFFFu)
        {
            continue;
        }

        ClassCode = (UINT8)((ClassRegister >> 24) & 0xFFu);
        Subclass = (UINT8)((ClassRegister >> 16) & 0xFFu);
        ProgIf = (UINT8)((ClassRegister >> 8) & 0xFFu);
        Kind = ClassCode == 0x03u ? KERNEL_PCI_KIND_GPU
            : ClassCode == 0x04u ? KERNEL_PCI_KIND_AUDIO
            : 0u;

        if (Kind == 0u)
        {
            continue;
        }

        if (Kind == KERNEL_PCI_KIND_GPU)
        {
            BootInfo->GpuCount++;
        }
        else
        {
            BootInfo->AudioCount++;
        }

        Segment = 0u;
        Bus = 0u;
        Device = 0u;
        Function = 0u;
        if (PciIo->GetLocation != 0)
        {
            (void)PciIo->GetLocation(PciIo, &Segment, &Bus, &Device, &Function);
        }

        if (BootInfo->PciDeviceCount < (sizeof(BootInfo->PciDevices) / sizeof(BootInfo->PciDevices[0])))
        {
            PCI_DEVICE_INFO* Entry = &BootInfo->PciDevices[BootInfo->PciDeviceCount++];
            Entry->VendorId = VendorId;
            Entry->DeviceId = DeviceId;
            Entry->ClassCode = ClassCode;
            Entry->Subclass = Subclass;
            Entry->ProgIf = ProgIf;
            Entry->Kind = Kind;
            Entry->Segment = (UINT8)Segment;
            Entry->Bus = (UINT8)Bus;
            Entry->Device = (UINT8)Device;
            Entry->Function = (UINT8)Function;
        }
    }

    (void)FreePool(Handles);
}

static EFI_STATUS ExitBootServicesForKernel(EFI_HANDLE ImageHandle, EFI_GET_MEMORY_MAP GetMemoryMap, EFI_ALLOCATE_POOL AllocatePool, EFI_EXIT_BOOT_SERVICES ExitBootServices, KERNEL_BOOT_INFO* BootInfo)
{
    EFI_STATUS Status;
    void* MemoryMap;
    UINTN MemoryMapSize;
    UINTN MapKey;
    UINTN DescriptorSize;
    UINT32 DescriptorVersion;

    MemoryMap = 0;
    MemoryMapSize = 0u;
    MapKey = 0u;
    DescriptorSize = 0u;
    DescriptorVersion = 0u;

    Status = GetMemoryMap(&MemoryMapSize, 0, &MapKey, &DescriptorSize, &DescriptorVersion);
    if (Status != EFI_BUFFER_TOO_SMALL)
    {
        return Status;
    }

    MemoryMapSize += DescriptorSize * 8u;
    Status = AllocatePool(EFI_BOOT_SERVICES_DATA, MemoryMapSize, &MemoryMap);
    if (Status != EFI_SUCCESS)
    {
        return Status;
    }

    for (;;)
    {
        UINTN CurrentMapSize = MemoryMapSize;

        Status = GetMemoryMap(&CurrentMapSize, MemoryMap, &MapKey, &DescriptorSize, &DescriptorVersion);
        if (Status != EFI_SUCCESS)
        {
            return Status;
        }

        BootInfo->MemoryMap = MemoryMap;
        BootInfo->MemoryMapSize = CurrentMapSize;
        BootInfo->MemoryMapKey = MapKey;
        BootInfo->MemoryDescriptorSize = DescriptorSize;
        BootInfo->MemoryDescriptorVersion = DescriptorVersion;
        BootInfo->MemoryDescriptorCount = DescriptorSize == 0u ? 0u : (CurrentMapSize / DescriptorSize);

        Status = ExitBootServices(ImageHandle, MapKey);
        if (Status == EFI_SUCCESS)
        {
            return EFI_SUCCESS;
        }

        if (Status != EFI_INVALID_PARAMETER)
        {
            return Status;
        }
    }
}

static void DrawKernelStatusScreen(const KERNEL_BOOT_INFO* BootInfo)
{
    UINT32 Black;
    INT32 startY;
    INT32 cursorY;
    INT32 finalY;
    INT32 bottomMargin;
    UINTN deviceIndex;
    char line[192];
    char* cursor;

    Black = ComposePixel(0u, 0u, 0u, BootInfo->Framebuffer.PixelFormat, BootInfo->Framebuffer.RedMask, BootInfo->Framebuffer.GreenMask, BootInfo->Framebuffer.BlueMask);
    ClearScreen(&BootInfo->Framebuffer, Black, 0u);

    cursor = line;
    AppendText(&cursor, "Framebuffer ");
    AppendInt(&cursor, (INT32)BootInfo->Framebuffer.ScreenWidth);
    AppendText(&cursor, "x");
    AppendInt(&cursor, (INT32)BootInfo->Framebuffer.ScreenHeight);
    AppendText(&cursor, "  scan ");
    AppendInt(&cursor, (INT32)BootInfo->Framebuffer.PixelsPerScanLine);
    *cursor = 0;
    {
        char framebufferLine[192];
        char memoryLine[192];
        char pciLine[192];

        for (deviceIndex = 0u; deviceIndex < sizeof(framebufferLine); deviceIndex++) { framebufferLine[deviceIndex] = 0; memoryLine[deviceIndex] = 0; pciLine[deviceIndex] = 0; }
        {
            char* write = framebufferLine;
            AppendText(&write, line);
            *write = 0;
        }

        cursor = memoryLine;
        AppendText(&cursor, "Memory descriptors ");
        AppendInt(&cursor, (INT32)BootInfo->MemoryDescriptorCount);
        AppendText(&cursor, "  size ");
        AppendInt(&cursor, (INT32)BootInfo->MemoryDescriptorSize);
        *cursor = 0;

        cursor = pciLine;
        AppendText(&cursor, "PCI display adapters ");
        AppendInt(&cursor, (INT32)BootInfo->GpuCount);
        AppendText(&cursor, "  audio controllers ");
        AppendInt(&cursor, (INT32)BootInfo->AudioCount);
        *cursor = 0;

        finalY = 16;
        finalY = AdvanceConsoleBlockY(finalY, "root@dragon:~# neofetch");
        finalY = AdvanceIconConsoleBlockY(finalY, gConsoleIconText);
        finalY = AdvanceConsoleBlockY(finalY, "Dragon OS Kernel Console");
        finalY = AdvanceConsoleBlockY(finalY, "UEFI services exited. Framebuffer ownership handed to kernel.");
        finalY = AdvanceConsoleBlockY(finalY, gRuntimeSummaryText);
        finalY = AdvanceConsoleBlockY(finalY, gRuntimeVideoText);
        finalY = AdvanceConsoleBlockY(finalY, framebufferLine);
        finalY = AdvanceConsoleBlockY(finalY, memoryLine);
        finalY = AdvanceConsoleBlockY(finalY, pciLine);
        finalY = AdvanceConsoleBlockY(finalY, "Kernel init log");
        finalY = AdvanceConsoleBlockY(finalY, "[ OK ] UEFI GOP framebuffer online");
        finalY = AdvanceConsoleBlockY(finalY, "[INFO] native GPU/monitor driver pending");
        finalY = AdvanceConsoleBlockY(finalY, "[ OK ] memory map captured");
        finalY = AdvanceConsoleBlockY(finalY, "[ OK ] runtime registry preloaded");
        finalY = AdvanceConsoleBlockY(finalY, "[ OK ] pci enumeration complete");
        finalY = AdvanceConsoleBlockY(finalY, "Detected PCI devices");

        for (deviceIndex = 0u; deviceIndex < BootInfo->PciDeviceCount && deviceIndex < 8u; deviceIndex++)
        {
            const PCI_DEVICE_INFO* DeviceInfo = &BootInfo->PciDevices[deviceIndex];
            char* lineCursor = line;
            line[0] = 0;
            AppendText(&lineCursor, DeviceInfo->Kind == KERNEL_PCI_KIND_GPU ? "GPU   " : "AUDIO ");
            AppendInt(&lineCursor, (INT32)DeviceInfo->Segment);
            AppendText(&lineCursor, ":");
            AppendInt(&lineCursor, (INT32)DeviceInfo->Bus);
            AppendText(&lineCursor, ":");
            AppendInt(&lineCursor, (INT32)DeviceInfo->Device);
            AppendText(&lineCursor, ".");
            AppendInt(&lineCursor, (INT32)DeviceInfo->Function);
            AppendText(&lineCursor, "  VEN ");
            AppendUIntHexFixed(&lineCursor, (UINT32)DeviceInfo->VendorId, 4u);
            AppendText(&lineCursor, " DEV ");
            AppendUIntHexFixed(&lineCursor, (UINT32)DeviceInfo->DeviceId, 4u);
            AppendText(&lineCursor, " CLS ");
            AppendUIntHexFixed(&lineCursor, (UINT32)DeviceInfo->ClassCode, 2u);
            AppendUIntHexFixed(&lineCursor, (UINT32)DeviceInfo->Subclass, 2u);
            *lineCursor = 0;
            finalY = AdvanceConsoleBlockY(finalY, line);
        }

        finalY = AdvanceConsoleBlockY(finalY, "Runtime registry");
        finalY = AdvanceConsoleBlockY(finalY, gRuntimeRegistryText);
        finalY = AdvanceConsoleBlockY(finalY, "root@dragon:~# sudo <pending keyboard/input driver>");

        bottomMargin = (INT32)BootInfo->Framebuffer.ScreenHeight - 16;
        startY = finalY > bottomMargin ? 16 - (finalY - bottomMargin) : 16;
        cursorY = startY;

        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "root@dragon:~# neofetch");
        cursorY = DrawIconConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, gConsoleIconText);
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "Dragon OS Kernel Console");
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "UEFI services exited. Framebuffer ownership handed to kernel.");
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, gRuntimeSummaryText);
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, gRuntimeVideoText);
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, framebufferLine);
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, memoryLine);
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, pciLine);
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "Kernel init log");
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "[ OK ] UEFI GOP framebuffer online");
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "[INFO] native GPU/monitor driver pending");
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "[ OK ] memory map captured");
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "[ OK ] runtime registry preloaded");
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "[ OK ] pci enumeration complete");
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "Detected PCI devices");

        for (deviceIndex = 0u; deviceIndex < BootInfo->PciDeviceCount && deviceIndex < 8u; deviceIndex++)
        {
            const PCI_DEVICE_INFO* DeviceInfo = &BootInfo->PciDevices[deviceIndex];
            char* lineCursor = line;
            line[0] = 0;
            AppendText(&lineCursor, DeviceInfo->Kind == KERNEL_PCI_KIND_GPU ? "GPU   " : "AUDIO ");
            AppendInt(&lineCursor, (INT32)DeviceInfo->Segment);
            AppendText(&lineCursor, ":");
            AppendInt(&lineCursor, (INT32)DeviceInfo->Bus);
            AppendText(&lineCursor, ":");
            AppendInt(&lineCursor, (INT32)DeviceInfo->Device);
            AppendText(&lineCursor, ".");
            AppendInt(&lineCursor, (INT32)DeviceInfo->Function);
            AppendText(&lineCursor, "  VEN ");
            AppendUIntHexFixed(&lineCursor, (UINT32)DeviceInfo->VendorId, 4u);
            AppendText(&lineCursor, " DEV ");
            AppendUIntHexFixed(&lineCursor, (UINT32)DeviceInfo->DeviceId, 4u);
            AppendText(&lineCursor, " CLS ");
            AppendUIntHexFixed(&lineCursor, (UINT32)DeviceInfo->ClassCode, 2u);
            AppendUIntHexFixed(&lineCursor, (UINT32)DeviceInfo->Subclass, 2u);
            *lineCursor = 0;
            cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, line);
        }

        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "Runtime registry");
        cursorY = DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, gRuntimeRegistryText);
        (void)DrawConsoleBlock(&BootInfo->Framebuffer, 16, cursorY, "root@dragon:~# sudo <pending keyboard/input driver>");
    }
}


EFI_STATUS EfiMain(EFI_HANDLE imageHandle, void* systemTable)
{
    UINT8* bootServicesBytes;
    EFI_LOCATE_PROTOCOL locateProtocol;
    EFI_GET_MEMORY_MAP getMemoryMap;
    EFI_ALLOCATE_POOL allocatePool;
    EFI_FREE_POOL freePool;
    EFI_HANDLE_PROTOCOL handleProtocol;
    EFI_LOCATE_HANDLE_BUFFER locateHandleBuffer;
    EFI_EXIT_BOOT_SERVICES exitBootServices;
    EFI_STALL stall;
    FRAMEBUFFER_INFO framebufferInfo;
    KERNEL_BOOT_INFO bootInfo;
    EFI_STATUS status;
    UINTN shadowFramebufferBytes;
    UINT32 shadowPixelCount;

    status = InitializeFramebuffer(systemTable, &framebufferInfo, &stall);
    if (status != EFI_SUCCESS)
    {
        return status;
    }

    ClearScreen(&framebufferInfo, ComposePixel(0u, 0u, 0u, framebufferInfo.PixelFormat, framebufferInfo.RedMask, framebufferInfo.GreenMask, framebufferInfo.BlueMask),0u);
    PlaySplashSequence(&framebufferInfo, stall);
    ClearScreen(&framebufferInfo, ComposePixel(0u, 0u, 0u, framebufferInfo.PixelFormat, framebufferInfo.RedMask, framebufferInfo.GreenMask, framebufferInfo.BlueMask),0u);

    bootInfo.Framebuffer = framebufferInfo;
    bootInfo.MemoryMap = 0;
    bootInfo.MemoryMapSize = 0u;
    bootInfo.MemoryMapKey = 0u;
    bootInfo.MemoryDescriptorSize = 0u;
    bootInfo.MemoryDescriptorVersion = 0u;
    bootInfo.MemoryDescriptorCount = 0u;
    bootInfo.ShadowFramebuffer = 0;
    bootInfo.ComposeFramebuffer = 0;
    bootInfo.TscTicksPerSecond = 0u;
    bootInfo.GpuCount = 0u;
    bootInfo.AudioCount = 0u;
    bootInfo.PciDeviceCount = 0u;

    bootServicesBytes = *(UINT8**)((UINT8*)systemTable + EFI_SYSTEM_TABLE_BOOT_SERVICES_OFFSET);
    locateProtocol = *(EFI_LOCATE_PROTOCOL*)(bootServicesBytes + EFI_BOOT_SERVICES_LOCATE_PROTOCOL_OFFSET);
    getMemoryMap = *(EFI_GET_MEMORY_MAP*)(bootServicesBytes + EFI_BOOT_SERVICES_GET_MEMORY_MAP_OFFSET);
    allocatePool = *(EFI_ALLOCATE_POOL*)(bootServicesBytes + EFI_BOOT_SERVICES_ALLOCATE_POOL_OFFSET);
    freePool = *(EFI_FREE_POOL*)(bootServicesBytes + EFI_BOOT_SERVICES_FREE_POOL_OFFSET);
    handleProtocol = *(EFI_HANDLE_PROTOCOL*)(bootServicesBytes + EFI_BOOT_SERVICES_HANDLE_PROTOCOL_OFFSET);
    locateHandleBuffer = *(EFI_LOCATE_HANDLE_BUFFER*)(bootServicesBytes + EFI_BOOT_SERVICES_LOCATE_HANDLE_BUFFER_OFFSET);
    exitBootServices = *(EFI_EXIT_BOOT_SERVICES*)(bootServicesBytes + EFI_BOOT_SERVICES_EXIT_BOOT_SERVICES_OFFSET);
    bootInfo.TscTicksPerSecond = EstimateTscTicksPerSecond(stall);

    shadowPixelCount = framebufferInfo.PixelsPerScanLine * framebufferInfo.ScreenHeight;
    shadowFramebufferBytes = (UINTN)shadowPixelCount * sizeof(UINT32);
    status = allocatePool(EFI_BOOT_SERVICES_DATA, shadowFramebufferBytes, (void**)&bootInfo.ShadowFramebuffer);
    if (status == EFI_SUCCESS && bootInfo.ShadowFramebuffer != 0)
    {
        CopyPixels32(bootInfo.ShadowFramebuffer, framebufferInfo.Framebuffer, shadowPixelCount);
    }
    else
    {
        bootInfo.ShadowFramebuffer = 0;
    }

    status = allocatePool(EFI_BOOT_SERVICES_DATA, shadowFramebufferBytes, (void**)&bootInfo.ComposeFramebuffer);
    if (status == EFI_SUCCESS && bootInfo.ComposeFramebuffer != 0)
    {
        if (bootInfo.ShadowFramebuffer != 0)
        {
            CopyPixels32(bootInfo.ComposeFramebuffer, bootInfo.ShadowFramebuffer, shadowPixelCount);
        }
        else
        {
            CopyPixels32(bootInfo.ComposeFramebuffer, framebufferInfo.Framebuffer, shadowPixelCount);
        }
    }
    else
    {
        bootInfo.ComposeFramebuffer = 0;
    }

    DrawTextString(&framebufferInfo, 24, 18, "Dragon OS Loader", 255u, 255u, 255u);
    DrawTextString(&framebufferInfo, 24, 52, "Collecting framebuffer, PCI, and memory-map info...", 180u, 220u, 255u);
    DrawTextString(&framebufferInfo, 24, 86, gRuntimeSummaryText, 180u, 180u, 255u);
    DrawTextString(&framebufferInfo, 24, 114, gRuntimeVideoText, 255u, 200u, 160u);
    (void)stall(250000u);
    PreserveUiSubsystemReferences();

    (void)locateProtocol;
    CollectPciDevices(locateHandleBuffer, handleProtocol, freePool, &bootInfo);
    status = ExitBootServicesForKernel(imageHandle, getMemoryMap, allocatePool, exitBootServices, &bootInfo);
    if (status != EFI_SUCCESS)
    {
        return status;
    }

    RunKernelMouseLoop(&bootInfo);
    for (;;)
    {
    }
}
