#ifndef MAINLIGHT_INCLUDED
#define MAINLIGHT_INCLUDED

void GetMainLightData_float(out half3 direction, out half3 color)
{
#ifdef SHADERGRAPH_PREVIEW
    
    direction = half3(0.0, 1.0, 0.0);
    color = half3(1.0, 1.0, 1.0);
    
#else

#if defined(UNIVERSAL_LIGHTING_INCLUDED)
    Light mainLight = GetMainLight();
    direction = mainLight.direction;
    color = mainLight.color;
    
    #endif

#endif
}

#endif
