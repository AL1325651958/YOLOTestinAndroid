; ModuleID = 'marshal_methods.arm64-v8a.ll'
source_filename = "marshal_methods.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [116 x ptr] zeroinitializer, align 8

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [232 x i64] [
	i64 14005105541811523, ; 0: System.Numerics.Tensors => 0x31c191bcdc8d43 => 53
	i64 98382396393917666, ; 1: Microsoft.Extensions.Primitives.dll => 0x15d8644ad360ce2 => 42
	i64 120698629574877762, ; 2: Mono.Android => 0x1accec39cafe242 => 115
	i64 131669012237370309, ; 3: Microsoft.Maui.Essentials.dll => 0x1d3c844de55c3c5 => 46
	i64 196720943101637631, ; 4: System.Linq.Expressions.dll => 0x2bae4a7cd73f3ff => 93
	i64 210515253464952879, ; 5: Xamarin.AndroidX.Collection.dll => 0x2ebe681f694702f => 58
	i64 232391251801502327, ; 6: Xamarin.AndroidX.SavedState.dll => 0x3399e9cbc897277 => 75
	i64 382336037851962543, ; 7: PictureYOLO.dll => 0x54e5499c8ff1caf => 82
	i64 545109961164950392, ; 8: fi/Microsoft.Maui.Controls.resources.dll => 0x7909e9f1ec38b78 => 7
	i64 750875890346172408, ; 9: System.Threading.Thread => 0xa6ba5a4da7d1ff8 => 108
	i64 799765834175365804, ; 10: System.ComponentModel.dll => 0xb1956c9f18442ac => 88
	i64 849051935479314978, ; 11: hi/Microsoft.Maui.Controls.resources.dll => 0xbc8703ca21a3a22 => 10
	i64 872800313462103108, ; 12: Xamarin.AndroidX.DrawerLayout => 0xc1ccf42c3c21c44 => 63
	i64 1120440138749646132, ; 13: Xamarin.Google.Android.Material.dll => 0xf8c9a5eae431534 => 79
	i64 1121665720830085036, ; 14: nb/Microsoft.Maui.Controls.resources.dll => 0xf90f507becf47ac => 18
	i64 1369545283391376210, ; 15: Xamarin.AndroidX.Navigation.Fragment.dll => 0x13019a2dd85acb52 => 71
	i64 1476839205573959279, ; 16: System.Net.Primitives.dll => 0x147ec96ece9b1e6f => 97
	i64 1486715745332614827, ; 17: Microsoft.Maui.Controls.dll => 0x14a1e017ea87d6ab => 43
	i64 1513467482682125403, ; 18: Mono.Android.Runtime => 0x1500eaa8245f6c5b => 114
	i64 1537168428375924959, ; 19: System.Threading.Thread.dll => 0x15551e8a954ae0df => 108
	i64 1556147632182429976, ; 20: ko/Microsoft.Maui.Controls.resources.dll => 0x15988c06d24c8918 => 16
	i64 1624659445732251991, ; 21: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0x168bf32877da9957 => 56
	i64 1628611045998245443, ; 22: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0x1699fd1e1a00b643 => 68
	i64 1743969030606105336, ; 23: System.Memory.dll => 0x1833d297e88f2af8 => 95
	i64 1767386781656293639, ; 24: System.Private.Uri.dll => 0x188704e9f5582107 => 101
	i64 1795316252682057001, ; 25: Xamarin.AndroidX.AppCompat.dll => 0x18ea3e9eac997529 => 55
	i64 1835311033149317475, ; 26: es\Microsoft.Maui.Controls.resources => 0x197855a927386163 => 6
	i64 1836611346387731153, ; 27: Xamarin.AndroidX.SavedState => 0x197cf449ebe482d1 => 75
	i64 1881198190668717030, ; 28: tr\Microsoft.Maui.Controls.resources => 0x1a1b5bc992ea9be6 => 28
	i64 1920760634179481754, ; 29: Microsoft.Maui.Controls.Xaml => 0x1aa7e99ec2d2709a => 44
	i64 1959996714666907089, ; 30: tr/Microsoft.Maui.Controls.resources.dll => 0x1b334ea0a2a755d1 => 28
	i64 1981742497975770890, ; 31: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x1b80904d5c241f0a => 67
	i64 1983698669889758782, ; 32: cs/Microsoft.Maui.Controls.resources.dll => 0x1b87836e2031a63e => 2
	i64 2019660174692588140, ; 33: pl/Microsoft.Maui.Controls.resources.dll => 0x1c07463a6f8e1a6c => 20
	i64 2262844636196693701, ; 34: Xamarin.AndroidX.DrawerLayout.dll => 0x1f673d352266e6c5 => 63
	i64 2287834202362508563, ; 35: System.Collections.Concurrent => 0x1fc00515e8ce7513 => 83
	i64 2302323944321350744, ; 36: ru/Microsoft.Maui.Controls.resources.dll => 0x1ff37f6ddb267c58 => 24
	i64 2329709569556905518, ; 37: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x2054ca829b447e2e => 66
	i64 2470498323731680442, ; 38: Xamarin.AndroidX.CoordinatorLayout => 0x2248f922dc398cba => 59
	i64 2497223385847772520, ; 39: System.Runtime => 0x22a7eb7046413568 => 105
	i64 2547086958574651984, ; 40: Xamarin.AndroidX.Activity.dll => 0x2359121801df4a50 => 54
	i64 2602673633151553063, ; 41: th\Microsoft.Maui.Controls.resources => 0x241e8de13a460e27 => 27
	i64 2656907746661064104, ; 42: Microsoft.Extensions.DependencyInjection => 0x24df3b84c8b75da8 => 37
	i64 2662981627730767622, ; 43: cs\Microsoft.Maui.Controls.resources => 0x24f4cfae6c48af06 => 2
	i64 2895129759130297543, ; 44: fi\Microsoft.Maui.Controls.resources => 0x282d912d479fa4c7 => 7
	i64 3017704767998173186, ; 45: Xamarin.Google.Android.Material => 0x29e10a7f7d88a002 => 79
	i64 3289520064315143713, ; 46: Xamarin.AndroidX.Lifecycle.Common => 0x2da6b911e3063621 => 65
	i64 3311221304742556517, ; 47: System.Numerics.Vectors.dll => 0x2df3d23ba9e2b365 => 99
	i64 3344514922410554693, ; 48: Xamarin.KotlinX.Coroutines.Core.Jvm => 0x2e6a1a9a18463545 => 81
	i64 3414639567687375782, ; 49: SkiaSharp.Views.Maui.Controls => 0x2f633c9863ffdba6 => 51
	i64 3429672777697402584, ; 50: Microsoft.Maui.Essentials => 0x2f98a5385a7b1ed8 => 46
	i64 3494946837667399002, ; 51: Microsoft.Extensions.Configuration => 0x30808ba1c00a455a => 35
	i64 3522470458906976663, ; 52: Xamarin.AndroidX.SwipeRefreshLayout => 0x30e2543832f52197 => 76
	i64 3551103847008531295, ; 53: System.Private.CoreLib.dll => 0x31480e226177735f => 112
	i64 3567343442040498961, ; 54: pt\Microsoft.Maui.Controls.resources => 0x3181bff5bea4ab11 => 22
	i64 3571415421602489686, ; 55: System.Runtime.dll => 0x319037675df7e556 => 105
	i64 3638003163729360188, ; 56: Microsoft.Extensions.Configuration.Abstractions => 0x327cc89a39d5f53c => 36
	i64 3647754201059316852, ; 57: System.Xml.ReaderWriter => 0x329f6d1e86145474 => 110
	i64 3655542548057982301, ; 58: Microsoft.Extensions.Configuration.dll => 0x32bb18945e52855d => 35
	i64 3727469159507183293, ; 59: Xamarin.AndroidX.RecyclerView => 0x33baa1739ba646bd => 74
	i64 3869221888984012293, ; 60: Microsoft.Extensions.Logging.dll => 0x35b23cceda0ed605 => 39
	i64 3890352374528606784, ; 61: Microsoft.Maui.Controls.Xaml.dll => 0x35fd4edf66e00240 => 44
	i64 3933965368022646939, ; 62: System.Net.Requests => 0x369840a8bfadc09b => 98
	i64 3966267475168208030, ; 63: System.Memory => 0x370b03412596249e => 95
	i64 4073500526318903918, ; 64: System.Private.Xml.dll => 0x3887fb25779ae26e => 102
	i64 4120493066591692148, ; 65: zh-Hant\Microsoft.Maui.Controls.resources => 0x392eee9cdda86574 => 33
	i64 4154383907710350974, ; 66: System.ComponentModel => 0x39a7562737acb67e => 88
	i64 4187479170553454871, ; 67: System.Linq.Expressions => 0x3a1cea1e912fa117 => 93
	i64 4205801962323029395, ; 68: System.ComponentModel.TypeConverter => 0x3a5e0299f7e7ad93 => 87
	i64 4356591372459378815, ; 69: vi/Microsoft.Maui.Controls.resources.dll => 0x3c75b8c562f9087f => 30
	i64 4679594760078841447, ; 70: ar/Microsoft.Maui.Controls.resources.dll => 0x40f142a407475667 => 0
	i64 4794310189461587505, ; 71: Xamarin.AndroidX.Activity => 0x4288cfb749e4c631 => 54
	i64 4795410492532947900, ; 72: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0x428cb86f8f9b7bbc => 76
	i64 4853321196694829351, ; 73: System.Runtime.Loader.dll => 0x435a75ea15de7927 => 104
	i64 5290786973231294105, ; 74: System.Runtime.Loader => 0x496ca6b869b72699 => 104
	i64 5471532531798518949, ; 75: sv\Microsoft.Maui.Controls.resources => 0x4beec9d926d82ca5 => 26
	i64 5522859530602327440, ; 76: uk\Microsoft.Maui.Controls.resources => 0x4ca5237b51eead90 => 29
	i64 5570799893513421663, ; 77: System.IO.Compression.Brotli => 0x4d4f74fcdfa6c35f => 91
	i64 5573260873512690141, ; 78: System.Security.Cryptography.dll => 0x4d58333c6e4ea1dd => 106
	i64 5692067934154308417, ; 79: Xamarin.AndroidX.ViewPager2.dll => 0x4efe49a0d4a8bb41 => 78
	i64 6005502812218439634, ; 80: PictureYOLO => 0x5357d4fd3be3b7d2 => 82
	i64 6068057819846744445, ; 81: ro/Microsoft.Maui.Controls.resources.dll => 0x5436126fec7f197d => 23
	i64 6200764641006662125, ; 82: ro\Microsoft.Maui.Controls.resources => 0x560d8a96830131ed => 23
	i64 6357457916754632952, ; 83: _Microsoft.Android.Resource.Designer => 0x583a3a4ac2a7a0f8 => 34
	i64 6401687960814735282, ; 84: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0x58d75d486341cfb2 => 66
	i64 6478287442656530074, ; 85: hr\Microsoft.Maui.Controls.resources => 0x59e7801b0c6a8e9a => 11
	i64 6548213210057960872, ; 86: Xamarin.AndroidX.CustomView.dll => 0x5adfed387b066da8 => 62
	i64 6560151584539558821, ; 87: Microsoft.Extensions.Options => 0x5b0a571be53243a5 => 41
	i64 6671798237668743565, ; 88: SkiaSharp => 0x5c96fd260152998d => 49
	i64 6743165466166707109, ; 89: nl\Microsoft.Maui.Controls.resources => 0x5d948943c08c43a5 => 19
	i64 6777482997383978746, ; 90: pt/Microsoft.Maui.Controls.resources.dll => 0x5e0e74e0a2525efa => 22
	i64 6894844156784520562, ; 91: System.Numerics.Vectors => 0x5faf683aead1ad72 => 99
	i64 7220009545223068405, ; 92: sv/Microsoft.Maui.Controls.resources.dll => 0x6432a06d99f35af5 => 26
	i64 7270811800166795866, ; 93: System.Linq => 0x64e71ccf51a90a5a => 94
	i64 7314237870106916923, ; 94: SkiaSharp.Views.Maui.Core.dll => 0x65816497226eb83b => 52
	i64 7377312882064240630, ; 95: System.ComponentModel.TypeConverter.dll => 0x66617afac45a2ff6 => 87
	i64 7489048572193775167, ; 96: System.ObjectModel => 0x67ee71ff6b419e3f => 100
	i64 7654504624184590948, ; 97: System.Net.Http => 0x6a3a4366801b8264 => 96
	i64 7708790323521193081, ; 98: ms/Microsoft.Maui.Controls.resources.dll => 0x6afb1ff4d1730479 => 17
	i64 7714652370974252055, ; 99: System.Private.CoreLib => 0x6b0ff375198b9c17 => 112
	i64 7723873813026311384, ; 100: SkiaSharp.Views.Maui.Controls.dll => 0x6b30b64f63600cd8 => 51
	i64 7735352534559001595, ; 101: Xamarin.Kotlin.StdLib.dll => 0x6b597e2582ce8bfb => 80
	i64 7836164640616011524, ; 102: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x6cbfa6390d64d704 => 56
	i64 7927939710195668715, ; 103: SkiaSharp.Views.Android.dll => 0x6e05b32992ed16eb => 50
	i64 8064050204834738623, ; 104: System.Collections.dll => 0x6fe942efa61731bf => 85
	i64 8083354569033831015, ; 105: Xamarin.AndroidX.Lifecycle.Common.dll => 0x702dd82730cad267 => 65
	i64 8087206902342787202, ; 106: System.Diagnostics.DiagnosticSource => 0x703b87d46f3aa082 => 90
	i64 8167236081217502503, ; 107: Java.Interop.dll => 0x7157d9f1a9b8fd27 => 113
	i64 8185542183669246576, ; 108: System.Collections => 0x7198e33f4794aa70 => 85
	i64 8246048515196606205, ; 109: Microsoft.Maui.Graphics.dll => 0x726fd96f64ee56fd => 47
	i64 8368701292315763008, ; 110: System.Security.Cryptography => 0x7423997c6fd56140 => 106
	i64 8400357532724379117, ; 111: Xamarin.AndroidX.Navigation.UI.dll => 0x749410ab44503ded => 73
	i64 8563666267364444763, ; 112: System.Private.Uri => 0x76d841191140ca5b => 101
	i64 8614108721271900878, ; 113: pt-BR/Microsoft.Maui.Controls.resources.dll => 0x778b763e14018ace => 21
	i64 8626175481042262068, ; 114: Java.Interop => 0x77b654e585b55834 => 113
	i64 8639588376636138208, ; 115: Xamarin.AndroidX.Navigation.Runtime => 0x77e5fbdaa2fda2e0 => 72
	i64 8677882282824630478, ; 116: pt-BR\Microsoft.Maui.Controls.resources => 0x786e07f5766b00ce => 21
	i64 8725526185868997716, ; 117: System.Diagnostics.DiagnosticSource.dll => 0x79174bd613173454 => 90
	i64 8771373629875524235, ; 118: Microsoft.ML.OnnxRuntime.dll => 0x79ba2dd7f8f5928b => 48
	i64 9045785047181495996, ; 119: zh-HK\Microsoft.Maui.Controls.resources => 0x7d891592e3cb0ebc => 31
	i64 9312692141327339315, ; 120: Xamarin.AndroidX.ViewPager2 => 0x813d54296a634f33 => 78
	i64 9324707631942237306, ; 121: Xamarin.AndroidX.AppCompat => 0x8168042fd44a7c7a => 55
	i64 9659729154652888475, ; 122: System.Text.RegularExpressions => 0x860e407c9991dd9b => 107
	i64 9678050649315576968, ; 123: Xamarin.AndroidX.CoordinatorLayout.dll => 0x864f57c9feb18c88 => 59
	i64 9702891218465930390, ; 124: System.Collections.NonGeneric.dll => 0x86a79827b2eb3c96 => 84
	i64 9808709177481450983, ; 125: Mono.Android.dll => 0x881f890734e555e7 => 115
	i64 9956195530459977388, ; 126: Microsoft.Maui => 0x8a2b8315b36616ac => 45
	i64 9991543690424095600, ; 127: es/Microsoft.Maui.Controls.resources.dll => 0x8aa9180c89861370 => 6
	i64 10038780035334861115, ; 128: System.Net.Http.dll => 0x8b50e941206af13b => 96
	i64 10051358222726253779, ; 129: System.Private.Xml => 0x8b7d990c97ccccd3 => 102
	i64 10092835686693276772, ; 130: Microsoft.Maui.Controls => 0x8c10f49539bd0c64 => 43
	i64 10143853363526200146, ; 131: da\Microsoft.Maui.Controls.resources => 0x8cc634e3c2a16b52 => 3
	i64 10229024438826829339, ; 132: Xamarin.AndroidX.CustomView => 0x8df4cb880b10061b => 62
	i64 10406448008575299332, ; 133: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 0x906b2153fcb3af04 => 81
	i64 10430153318873392755, ; 134: Xamarin.AndroidX.Core => 0x90bf592ea44f6673 => 60
	i64 10506226065143327199, ; 135: ca\Microsoft.Maui.Controls.resources => 0x91cd9cf11ed169df => 1
	i64 10540451825959151825, ; 136: Microsoft.ML.OnnxRuntime => 0x924735170a69a8d1 => 48
	i64 10785150219063592792, ; 137: System.Net.Primitives => 0x95ac8cfb68830758 => 97
	i64 11002576679268595294, ; 138: Microsoft.Extensions.Logging.Abstractions => 0x98b1013215cd365e => 40
	i64 11009005086950030778, ; 139: Microsoft.Maui.dll => 0x98c7d7cc621ffdba => 45
	i64 11103970607964515343, ; 140: hu\Microsoft.Maui.Controls.resources => 0x9a193a6fc41a6c0f => 12
	i64 11162124722117608902, ; 141: Xamarin.AndroidX.ViewPager => 0x9ae7d54b986d05c6 => 77
	i64 11220793807500858938, ; 142: ja\Microsoft.Maui.Controls.resources => 0x9bb8448481fdd63a => 15
	i64 11226290749488709958, ; 143: Microsoft.Extensions.Options.dll => 0x9bcbcbf50c874146 => 41
	i64 11340910727871153756, ; 144: Xamarin.AndroidX.CursorAdapter => 0x9d630238642d465c => 61
	i64 11485890710487134646, ; 145: System.Runtime.InteropServices => 0x9f6614bf0f8b71b6 => 103
	i64 11518296021396496455, ; 146: id\Microsoft.Maui.Controls.resources => 0x9fd9353475222047 => 13
	i64 11529969570048099689, ; 147: Xamarin.AndroidX.ViewPager.dll => 0xa002ae3c4dc7c569 => 77
	i64 11530571088791430846, ; 148: Microsoft.Extensions.Logging => 0xa004d1504ccd66be => 39
	i64 11705530742807338875, ; 149: he/Microsoft.Maui.Controls.resources.dll => 0xa272663128721f7b => 9
	i64 12451044538927396471, ; 150: Xamarin.AndroidX.Fragment.dll => 0xaccaff0a2955b677 => 64
	i64 12466513435562512481, ; 151: Xamarin.AndroidX.Loader.dll => 0xad01f3eb52569061 => 69
	i64 12475113361194491050, ; 152: _Microsoft.Android.Resource.Designer.dll => 0xad2081818aba1caa => 34
	i64 12538491095302438457, ; 153: Xamarin.AndroidX.CardView.dll => 0xae01ab382ae67e39 => 57
	i64 12550732019250633519, ; 154: System.IO.Compression => 0xae2d28465e8e1b2f => 92
	i64 12681088699309157496, ; 155: it/Microsoft.Maui.Controls.resources.dll => 0xaffc46fc178aec78 => 14
	i64 12700543734426720211, ; 156: Xamarin.AndroidX.Collection => 0xb041653c70d157d3 => 58
	i64 12823819093633476069, ; 157: th/Microsoft.Maui.Controls.resources.dll => 0xb1f75b85abe525e5 => 27
	i64 12843321153144804894, ; 158: Microsoft.Extensions.Primitives => 0xb23ca48abd74d61e => 42
	i64 13221551921002590604, ; 159: ca/Microsoft.Maui.Controls.resources.dll => 0xb77c636bdebe318c => 1
	i64 13222659110913276082, ; 160: ja/Microsoft.Maui.Controls.resources.dll => 0xb78052679c1178b2 => 15
	i64 13343850469010654401, ; 161: Mono.Android.Runtime.dll => 0xb92ee14d854f44c1 => 114
	i64 13381594904270902445, ; 162: he\Microsoft.Maui.Controls.resources => 0xb9b4f9aaad3e94ad => 9
	i64 13465488254036897740, ; 163: Xamarin.Kotlin.StdLib => 0xbadf06394d106fcc => 80
	i64 13467053111158216594, ; 164: uk/Microsoft.Maui.Controls.resources.dll => 0xbae49573fde79792 => 29
	i64 13540124433173649601, ; 165: vi\Microsoft.Maui.Controls.resources => 0xbbe82f6eede718c1 => 30
	i64 13545416393490209236, ; 166: id/Microsoft.Maui.Controls.resources.dll => 0xbbfafc7174bc99d4 => 13
	i64 13572454107664307259, ; 167: Xamarin.AndroidX.RecyclerView.dll => 0xbc5b0b19d99f543b => 74
	i64 13717397318615465333, ; 168: System.ComponentModel.Primitives.dll => 0xbe5dfc2ef2f87d75 => 86
	i64 13755568601956062840, ; 169: fr/Microsoft.Maui.Controls.resources.dll => 0xbee598c36b1b9678 => 8
	i64 13814445057219246765, ; 170: hr/Microsoft.Maui.Controls.resources.dll => 0xbfb6c49664b43aad => 11
	i64 13881769479078963060, ; 171: System.Console.dll => 0xc0a5f3cade5c6774 => 89
	i64 13959074834287824816, ; 172: Xamarin.AndroidX.Fragment => 0xc1b8989a7ad20fb0 => 64
	i64 14100563506285742564, ; 173: da/Microsoft.Maui.Controls.resources.dll => 0xc3af43cd0cff89e4 => 3
	i64 14113375261243792383, ; 174: System.Numerics.Tensors.dll => 0xc3dcc8063438dbff => 53
	i64 14124974489674258913, ; 175: Xamarin.AndroidX.CardView => 0xc405fd76067d19e1 => 57
	i64 14125464355221830302, ; 176: System.Threading.dll => 0xc407bafdbc707a9e => 109
	i64 14461014870687870182, ; 177: System.Net.Requests.dll => 0xc8afd8683afdece6 => 98
	i64 14464374589798375073, ; 178: ru\Microsoft.Maui.Controls.resources => 0xc8bbc80dcb1e5ea1 => 24
	i64 14522721392235705434, ; 179: el/Microsoft.Maui.Controls.resources.dll => 0xc98b12295c2cf45a => 5
	i64 14552901170081803662, ; 180: SkiaSharp.Views.Maui.Core => 0xc9f64a827617ad8e => 52
	i64 14669215534098758659, ; 181: Microsoft.Extensions.DependencyInjection.dll => 0xcb9385ceb3993c03 => 37
	i64 14705122255218365489, ; 182: ko\Microsoft.Maui.Controls.resources => 0xcc1316c7b0fb5431 => 16
	i64 14744092281598614090, ; 183: zh-Hans\Microsoft.Maui.Controls.resources => 0xcc9d89d004439a4a => 32
	i64 14852515768018889994, ; 184: Xamarin.AndroidX.CursorAdapter.dll => 0xce1ebc6625a76d0a => 61
	i64 14892012299694389861, ; 185: zh-Hant/Microsoft.Maui.Controls.resources.dll => 0xceab0e490a083a65 => 33
	i64 14904040806490515477, ; 186: ar\Microsoft.Maui.Controls.resources => 0xced5ca2604cb2815 => 0
	i64 14954917835170835695, ; 187: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 0xcf8a8a895a82ecef => 38
	i64 14987728460634540364, ; 188: System.IO.Compression.dll => 0xcfff1ba06622494c => 92
	i64 15076659072870671916, ; 189: System.ObjectModel.dll => 0xd13b0d8c1620662c => 100
	i64 15111608613780139878, ; 190: ms\Microsoft.Maui.Controls.resources => 0xd1b737f831192f66 => 17
	i64 15115185479366240210, ; 191: System.IO.Compression.Brotli.dll => 0xd1c3ed1c1bc467d2 => 91
	i64 15133485256822086103, ; 192: System.Linq.dll => 0xd204f0a9127dd9d7 => 94
	i64 15227001540531775957, ; 193: Microsoft.Extensions.Configuration.Abstractions.dll => 0xd3512d3999b8e9d5 => 36
	i64 15370334346939861994, ; 194: Xamarin.AndroidX.Core.dll => 0xd54e65a72c560bea => 60
	i64 15391712275433856905, ; 195: Microsoft.Extensions.DependencyInjection.Abstractions => 0xd59a58c406411f89 => 38
	i64 15527772828719725935, ; 196: System.Console => 0xd77dbb1e38cd3d6f => 89
	i64 15536481058354060254, ; 197: de\Microsoft.Maui.Controls.resources => 0xd79cab34eec75bde => 4
	i64 15582737692548360875, ; 198: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xd841015ed86f6aab => 68
	i64 15609085926864131306, ; 199: System.dll => 0xd89e9cf3334914ea => 111
	i64 15661133872274321916, ; 200: System.Xml.ReaderWriter.dll => 0xd9578647d4bfb1fc => 110
	i64 15664356999916475676, ; 201: de/Microsoft.Maui.Controls.resources.dll => 0xd962f9b2b6ecd51c => 4
	i64 15743187114543869802, ; 202: hu/Microsoft.Maui.Controls.resources.dll => 0xda7b09450ae4ef6a => 12
	i64 15783653065526199428, ; 203: el\Microsoft.Maui.Controls.resources => 0xdb0accd674b1c484 => 5
	i64 16154507427712707110, ; 204: System => 0xe03056ea4e39aa26 => 111
	i64 16288847719894691167, ; 205: nb\Microsoft.Maui.Controls.resources => 0xe20d9cb300c12d5f => 18
	i64 16321164108206115771, ; 206: Microsoft.Extensions.Logging.Abstractions.dll => 0xe2806c487e7b0bbb => 40
	i64 16324796876805858114, ; 207: SkiaSharp.dll => 0xe28d5444586b6342 => 49
	i64 16649148416072044166, ; 208: Microsoft.Maui.Graphics => 0xe70da84600bb4e86 => 47
	i64 16677317093839702854, ; 209: Xamarin.AndroidX.Navigation.UI => 0xe771bb8960dd8b46 => 73
	i64 16890310621557459193, ; 210: System.Text.RegularExpressions.dll => 0xea66700587f088f9 => 107
	i64 16942731696432749159, ; 211: sk\Microsoft.Maui.Controls.resources => 0xeb20acb622a01a67 => 25
	i64 16998075588627545693, ; 212: Xamarin.AndroidX.Navigation.Fragment => 0xebe54bb02d623e5d => 71
	i64 17008137082415910100, ; 213: System.Collections.NonGeneric => 0xec090a90408c8cd4 => 84
	i64 17031351772568316411, ; 214: Xamarin.AndroidX.Navigation.Common.dll => 0xec5b843380a769fb => 70
	i64 17062143951396181894, ; 215: System.ComponentModel.Primitives => 0xecc8e986518c9786 => 86
	i64 17089008752050867324, ; 216: zh-Hans/Microsoft.Maui.Controls.resources.dll => 0xed285aeb25888c7c => 32
	i64 17342750010158924305, ; 217: hi\Microsoft.Maui.Controls.resources => 0xf0add33f97ecc211 => 10
	i64 17438153253682247751, ; 218: sk/Microsoft.Maui.Controls.resources.dll => 0xf200c3fe308d7847 => 25
	i64 17514990004910432069, ; 219: fr\Microsoft.Maui.Controls.resources => 0xf311be9c6f341f45 => 8
	i64 17623389608345532001, ; 220: pl\Microsoft.Maui.Controls.resources => 0xf492db79dfbef661 => 20
	i64 17671790519499593115, ; 221: SkiaSharp.Views.Android => 0xf53ecfd92be3959b => 50
	i64 17702523067201099846, ; 222: zh-HK/Microsoft.Maui.Controls.resources.dll => 0xf5abfef008ae1846 => 31
	i64 17704177640604968747, ; 223: Xamarin.AndroidX.Loader => 0xf5b1dfc36cac272b => 69
	i64 17710060891934109755, ; 224: Xamarin.AndroidX.Lifecycle.ViewModel => 0xf5c6c68c9e45303b => 67
	i64 17712670374920797664, ; 225: System.Runtime.InteropServices.dll => 0xf5d00bdc38bd3de0 => 103
	i64 18025913125965088385, ; 226: System.Threading => 0xfa28e87b91334681 => 109
	i64 18099568558057551825, ; 227: nl/Microsoft.Maui.Controls.resources.dll => 0xfb2e95b53ad977d1 => 19
	i64 18121036031235206392, ; 228: Xamarin.AndroidX.Navigation.Common => 0xfb7ada42d3d42cf8 => 70
	i64 18245806341561545090, ; 229: System.Collections.Concurrent.dll => 0xfd3620327d587182 => 83
	i64 18305135509493619199, ; 230: Xamarin.AndroidX.Navigation.Runtime.dll => 0xfe08e7c2d8c199ff => 72
	i64 18324163916253801303 ; 231: it\Microsoft.Maui.Controls.resources => 0xfe4c81ff0a56ab57 => 14
], align 8

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [232 x i32] [
	i32 53, ; 0
	i32 42, ; 1
	i32 115, ; 2
	i32 46, ; 3
	i32 93, ; 4
	i32 58, ; 5
	i32 75, ; 6
	i32 82, ; 7
	i32 7, ; 8
	i32 108, ; 9
	i32 88, ; 10
	i32 10, ; 11
	i32 63, ; 12
	i32 79, ; 13
	i32 18, ; 14
	i32 71, ; 15
	i32 97, ; 16
	i32 43, ; 17
	i32 114, ; 18
	i32 108, ; 19
	i32 16, ; 20
	i32 56, ; 21
	i32 68, ; 22
	i32 95, ; 23
	i32 101, ; 24
	i32 55, ; 25
	i32 6, ; 26
	i32 75, ; 27
	i32 28, ; 28
	i32 44, ; 29
	i32 28, ; 30
	i32 67, ; 31
	i32 2, ; 32
	i32 20, ; 33
	i32 63, ; 34
	i32 83, ; 35
	i32 24, ; 36
	i32 66, ; 37
	i32 59, ; 38
	i32 105, ; 39
	i32 54, ; 40
	i32 27, ; 41
	i32 37, ; 42
	i32 2, ; 43
	i32 7, ; 44
	i32 79, ; 45
	i32 65, ; 46
	i32 99, ; 47
	i32 81, ; 48
	i32 51, ; 49
	i32 46, ; 50
	i32 35, ; 51
	i32 76, ; 52
	i32 112, ; 53
	i32 22, ; 54
	i32 105, ; 55
	i32 36, ; 56
	i32 110, ; 57
	i32 35, ; 58
	i32 74, ; 59
	i32 39, ; 60
	i32 44, ; 61
	i32 98, ; 62
	i32 95, ; 63
	i32 102, ; 64
	i32 33, ; 65
	i32 88, ; 66
	i32 93, ; 67
	i32 87, ; 68
	i32 30, ; 69
	i32 0, ; 70
	i32 54, ; 71
	i32 76, ; 72
	i32 104, ; 73
	i32 104, ; 74
	i32 26, ; 75
	i32 29, ; 76
	i32 91, ; 77
	i32 106, ; 78
	i32 78, ; 79
	i32 82, ; 80
	i32 23, ; 81
	i32 23, ; 82
	i32 34, ; 83
	i32 66, ; 84
	i32 11, ; 85
	i32 62, ; 86
	i32 41, ; 87
	i32 49, ; 88
	i32 19, ; 89
	i32 22, ; 90
	i32 99, ; 91
	i32 26, ; 92
	i32 94, ; 93
	i32 52, ; 94
	i32 87, ; 95
	i32 100, ; 96
	i32 96, ; 97
	i32 17, ; 98
	i32 112, ; 99
	i32 51, ; 100
	i32 80, ; 101
	i32 56, ; 102
	i32 50, ; 103
	i32 85, ; 104
	i32 65, ; 105
	i32 90, ; 106
	i32 113, ; 107
	i32 85, ; 108
	i32 47, ; 109
	i32 106, ; 110
	i32 73, ; 111
	i32 101, ; 112
	i32 21, ; 113
	i32 113, ; 114
	i32 72, ; 115
	i32 21, ; 116
	i32 90, ; 117
	i32 48, ; 118
	i32 31, ; 119
	i32 78, ; 120
	i32 55, ; 121
	i32 107, ; 122
	i32 59, ; 123
	i32 84, ; 124
	i32 115, ; 125
	i32 45, ; 126
	i32 6, ; 127
	i32 96, ; 128
	i32 102, ; 129
	i32 43, ; 130
	i32 3, ; 131
	i32 62, ; 132
	i32 81, ; 133
	i32 60, ; 134
	i32 1, ; 135
	i32 48, ; 136
	i32 97, ; 137
	i32 40, ; 138
	i32 45, ; 139
	i32 12, ; 140
	i32 77, ; 141
	i32 15, ; 142
	i32 41, ; 143
	i32 61, ; 144
	i32 103, ; 145
	i32 13, ; 146
	i32 77, ; 147
	i32 39, ; 148
	i32 9, ; 149
	i32 64, ; 150
	i32 69, ; 151
	i32 34, ; 152
	i32 57, ; 153
	i32 92, ; 154
	i32 14, ; 155
	i32 58, ; 156
	i32 27, ; 157
	i32 42, ; 158
	i32 1, ; 159
	i32 15, ; 160
	i32 114, ; 161
	i32 9, ; 162
	i32 80, ; 163
	i32 29, ; 164
	i32 30, ; 165
	i32 13, ; 166
	i32 74, ; 167
	i32 86, ; 168
	i32 8, ; 169
	i32 11, ; 170
	i32 89, ; 171
	i32 64, ; 172
	i32 3, ; 173
	i32 53, ; 174
	i32 57, ; 175
	i32 109, ; 176
	i32 98, ; 177
	i32 24, ; 178
	i32 5, ; 179
	i32 52, ; 180
	i32 37, ; 181
	i32 16, ; 182
	i32 32, ; 183
	i32 61, ; 184
	i32 33, ; 185
	i32 0, ; 186
	i32 38, ; 187
	i32 92, ; 188
	i32 100, ; 189
	i32 17, ; 190
	i32 91, ; 191
	i32 94, ; 192
	i32 36, ; 193
	i32 60, ; 194
	i32 38, ; 195
	i32 89, ; 196
	i32 4, ; 197
	i32 68, ; 198
	i32 111, ; 199
	i32 110, ; 200
	i32 4, ; 201
	i32 12, ; 202
	i32 5, ; 203
	i32 111, ; 204
	i32 18, ; 205
	i32 40, ; 206
	i32 49, ; 207
	i32 47, ; 208
	i32 73, ; 209
	i32 107, ; 210
	i32 25, ; 211
	i32 71, ; 212
	i32 84, ; 213
	i32 70, ; 214
	i32 86, ; 215
	i32 32, ; 216
	i32 10, ; 217
	i32 25, ; 218
	i32 8, ; 219
	i32 20, ; 220
	i32 50, ; 221
	i32 31, ; 222
	i32 69, ; 223
	i32 67, ; 224
	i32 103, ; 225
	i32 109, ; 226
	i32 19, ; 227
	i32 70, ; 228
	i32 83, ; 229
	i32 72, ; 230
	i32 14 ; 231
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 8

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

; Functions

; Function attributes: "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" }

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!"Xamarin.Android remotes/origin/release/8.0.4xx @ 82d8938cf80f6d5fa6c28529ddfbdb753d805ab4"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
