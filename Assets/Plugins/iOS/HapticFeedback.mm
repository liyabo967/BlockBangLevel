#import <UIKit/UIKit.h>
#import <Foundation/Foundation.h>
#import <CoreHaptics/CoreHaptics.h>

static CHHapticEngine *hapticEngine = nil;


// ============================================================
// Haptic Engine Initialization
// ============================================================

static void InitHaptics()
{
    if (@available(iOS 13.0, *))
    {
        // 已经创建过 Engine
        if (hapticEngine != nil)
        {
            NSLog(@"[Haptics] Engine already exists: %p",
                  hapticEngine);
            return;
        }

        NSError *error = nil;

        // 创建 Engine
        hapticEngine =
            [[CHHapticEngine alloc] initAndReturnError:&error];

        if (hapticEngine == nil || error != nil)
        {
            NSLog(@"[Haptics] Engine init failed: %@",
                  error);

            hapticEngine = nil;
            return;
        }

        NSLog(@"[Haptics] Engine created: %p",
              hapticEngine);


        // ========================================================
        // Engine stopped callback
        // ========================================================

        hapticEngine.stoppedHandler =
        ^(CHHapticEngineStoppedReason reason)
        {
            NSLog(@"[Haptics] Engine stopped. reason=%ld",
                  (long)reason);
        };


        // ========================================================
        // Engine reset callback
        // ========================================================

        hapticEngine.resetHandler =
        ^{
            NSLog(@"[Haptics] Engine reset");
        };


        // ========================================================
        // 同步启动 Engine
        // ========================================================

        error = nil;

        BOOL success =
            [hapticEngine startAndReturnError:&error];

        if (!success || error != nil)
        {
            NSLog(@"[Haptics] Engine start failed: %@",
                  error);

            hapticEngine = nil;
            return;
        }

        NSLog(@"[Haptics] Engine start succeeded: %p",
              hapticEngine);
    }
}


// ============================================================
// External C Functions
// ============================================================

extern "C"
{

    // ============================================================
    // UIKit Impact Haptic
    // ============================================================

    void _TriggerHapticFeedback(int force)
    {
        @autoreleasepool
        {
            UIImpactFeedbackGenerator *generator = nil;

            switch (force)
            {
                case 0:
                    generator =
                        [[UIImpactFeedbackGenerator alloc]
                            initWithStyle:UIImpactFeedbackStyleLight];
                    break;

                case 1:
                    generator =
                        [[UIImpactFeedbackGenerator alloc]
                            initWithStyle:UIImpactFeedbackStyleMedium];
                    break;

                case 2:
                    generator =
                        [[UIImpactFeedbackGenerator alloc]
                            initWithStyle:UIImpactFeedbackStyleHeavy];
                    break;

                default:
                    generator =
                        [[UIImpactFeedbackGenerator alloc]
                            initWithStyle:UIImpactFeedbackStyleLight];
                    break;
            }

            if (generator == nil)
            {
                NSLog(@"[Haptics] Failed to create impact generator");
                return;
            }

            [generator prepare];
            [generator impactOccurred];
        }
    }


    // ============================================================
    // CoreHaptics Impact
    // ============================================================

    void _PlayImpactNative(float intensity,
                           float sharpness,
                           float duration)
    {
        @autoreleasepool
        {
            if (@available(iOS 13.0, *))
            {
                NSLog(@"[Haptics] _PlayImpactNative BEGIN "
                      @"intensity=%f "
                      @"sharpness=%f "
                      @"duration=%f",
                      intensity,
                      sharpness,
                      duration);


                // ------------------------------------------------
                // 参数限制
                // ------------------------------------------------

                if (duration <= 0.0f)
                {
                    NSLog(@"[Haptics] Invalid duration: %f",
                          duration);
                    return;
                }

                if (intensity < 0.0f)
                    intensity = 0.0f;

                if (intensity > 1.0f)
                    intensity = 1.0f;

                if (sharpness < 0.0f)
                    sharpness = 0.0f;

                if (sharpness > 1.0f)
                    sharpness = 1.0f;


                // ------------------------------------------------
                // 初始化 Engine
                // ------------------------------------------------

                InitHaptics();

                if (hapticEngine == nil)
                {
                    NSLog(@"[Haptics] Engine unavailable. Abort.");
                    return;
                }

                NSLog(@"[Haptics] Engine=%p",
                      hapticEngine);


                // ------------------------------------------------
                // Intensity
                // ------------------------------------------------

                CHHapticEventParameter *intensityParam =
                    [[CHHapticEventParameter alloc]
                        initWithParameterID:
                            CHHapticEventParameterIDHapticIntensity
                        value:intensity];


                // ------------------------------------------------
                // Sharpness
                // ------------------------------------------------

                CHHapticEventParameter *sharpnessParam =
                    [[CHHapticEventParameter alloc]
                        initWithParameterID:
                            CHHapticEventParameterIDHapticSharpness
                        value:sharpness];


                // ------------------------------------------------
                // Event
                // ------------------------------------------------

                CHHapticEvent *event =
                    [[CHHapticEvent alloc]
                        initWithEventType:
                            CHHapticEventTypeHapticContinuous
                        parameters:@[
                            intensityParam,
                            sharpnessParam
                        ]
                        relativeTime:0.0
                        duration:duration];

                if (event == nil)
                {
                    NSLog(@"[Haptics] Failed to create event");
                    return;
                }


                // ------------------------------------------------
                // Pattern
                // ------------------------------------------------

                NSError *patternError = nil;

                CHHapticPattern *pattern =
                    [[CHHapticPattern alloc]
                        initWithEvents:@[event]
                        parameters:@[]
                        error:&patternError];

                if (pattern == nil || patternError != nil)
                {
                    NSLog(@"[Haptics] Pattern creation failed: %@",
                          patternError);
                    return;
                }


                // ------------------------------------------------
                // Player
                // ------------------------------------------------

                NSError *playerError = nil;

                id<CHHapticPatternPlayer> player =
                    [hapticEngine
                        createPlayerWithPattern:pattern
                        error:&playerError];

                if (player == nil || playerError != nil)
                {
                    NSLog(@"[Haptics] Player creation failed: %@",
                          playerError);
                    return;
                }

                NSLog(@"[Haptics] Player created");


                // ------------------------------------------------
                // Start Player
                // ------------------------------------------------

                NSError *startError = nil;

                BOOL success =
                    [player startAtTime:0
                                  error:&startError];

                if (!success || startError != nil)
                {
                    NSLog(@"[Haptics] Player start failed: %@",
                          startError);
                    return;
                }

                NSLog(@"[Haptics] Player start succeeded");

                NSLog(@"[Haptics] _PlayImpactNative END");
            }
        }
    }

}
