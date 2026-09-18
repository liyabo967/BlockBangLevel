// In a file named HapticFeedback.mm
#import <UIKit/UIKit.h>

#import <Foundation/Foundation.h>
#import <CoreHaptics/CoreHaptics.h>

static CHHapticEngine *hapticEngine = nil;

static void InitHaptics()
{
    if (@available(iOS 13.0, *))
    {
        if (hapticEngine != nil)
            return;

        NSError *error = nil;

        hapticEngine =
            [[CHHapticEngine alloc] initAndReturnError:&error];

        if (error)
        {
            NSLog(@"Haptic Engine Error: %@", error);
            hapticEngine = nil;
            return;
        }

        [hapticEngine startWithCompletionHandler:
            ^(NSError *error)
        {
            if (error)
            {
                NSLog(@"Start Haptic Error: %@", error);
            }
        }];
    }
}


extern "C" {
    void _TriggerHapticFeedback(int force)
    {
        UIImpactFeedbackGenerator *generator;
        switch (force)
        {
            case 0:
                generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                break;
            case 1:
                generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                break;
            case 2:
                generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
                break;
            default:
                generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                break;
        }
        [generator prepare];
        [generator impactOccurred];
    }

    void _PlayImpactNative(float intensity, float sharpness, float duration)
    {
        if (@available(iOS 13.0, *))
        {
            InitHaptics();
            
            if (hapticEngine == nil)
                return;
            
            CHHapticEventParameter *intensityParam =
            [[CHHapticEventParameter alloc]
             initWithParameterID:CHHapticEventParameterIDHapticIntensity
             value:intensity];
            
            CHHapticEventParameter *sharpnessParam =
            [[CHHapticEventParameter alloc]
             initWithParameterID:CHHapticEventParameterIDHapticSharpness
             value:sharpness];
            
            CHHapticEvent *event =
            [[CHHapticEvent alloc]
             initWithEventType:CHHapticEventTypeHapticContinuous
             parameters:@[
                intensityParam,
                sharpnessParam
            ]
             relativeTime:0
             duration:duration];
            
            CHHapticPattern *pattern =
            [[CHHapticPattern alloc]
             initWithEvents:@[event]
             parameters:@[]
             error:nil];
            
            id<CHHapticPatternPlayer> player =
            [hapticEngine createPlayerWithPattern:pattern
                                            error:nil];
            
            [player startAtTime:0 error:nil];
        }
    }
}
