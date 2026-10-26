
#import <Foundation/Foundation.h>

extern "C" const char* GetDeviceCountryCodeNative()
{
    NSLocale *locale = [NSLocale autoupdatingCurrentLocale];

    NSString *countryCode =
        [locale objectForKey:NSLocaleCountryCode];

    if (countryCode == nil || countryCode.length == 0)
    {
        return "";
    }

    // 使用静态变量保证返回的字符串在函数返回后仍有效
    static NSString *cachedCountryCode = nil;
    cachedCountryCode = [countryCode copy];

    return [cachedCountryCode UTF8String];
}
