#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif
float2 shadowMapSize;
float4x4 LightViewProjection;
float4x4 WorldViewProjectionSun;
float4x4 InverseTransposeWorld;
float4x4 World;
float4x4 View;
float4x4 Projection;
float Time = 0;
float3 cameraPosition;
float3 sunPosition;
float KAmbient;
float3 ambientColor;
float4x4 WorldViewProjection;
float KDiffuse;
float3 diffuseColor;

float KSpecular;
float3 specularColor;
float shininess;

float KReflection;
float KFoam;
//static const float modulatedEpsilon = 0.000000000041200182749889791011810302734375;
//static const float maxEpsilon = 0.000000000000000023200045689009130001068115234375;
static const float modulatedEpsilon = 0.0000041200182749889791011810302734375;
static const float maxEpsilon = 0.0000002320045689009130001068115234375;
texture shadowMap;
sampler2D shadowMapSampler =
sampler_state
{
	Texture = <shadowMap>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};
struct DepthPassVertexShaderInput
{
	float4 Position : POSITION0;
};

struct DepthPassVertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 ScreenSpacePosition : TEXCOORD6;
};

texture baseTexture;
sampler2D textureSampler = sampler_state
{
    Texture = (baseTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};
texture foamTexture;
sampler2D foamSampler = sampler_state
{
    Texture = (foamTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};

texture normalTexture;
sampler2D normalSampler = sampler_state
{
    Texture = (normalTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

struct VS_INPUT
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
    float3 Normal : NORMAL;
};

struct VS_OUTPUT
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinates : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
    float4 ScreenSpacePosition : TEXCOORD3;
    float4 LightSpacePosition : TEXCOORD4;
};
texture environmentMap;
samplerCUBE environmentMapSampler = sampler_state
{
    Texture = (environmentMap);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};
float3 createWave(float steepness, float numWaves, float2 waveDir, float waveAmplitude, float waveLength, float peak, float speed, float4 position) {
    float3 wave = float3(0, 0, 0);

    float spaceMult = 2 * 3.14159265359 / waveLength;
    float timeMult = speed * 2 * 3.14159265359 / waveLength;

    wave.x = waveAmplitude * steepness * waveDir.x * cos(dot(position.xz, waveDir) * spaceMult + Time * timeMult);
    wave.y = 2 * waveAmplitude * pow(((sin(dot(position.xz, waveDir) * spaceMult + Time * timeMult) + 1) / 2), peak);
    wave.z = waveAmplitude * steepness * waveDir.y * cos(dot(position.xz, waveDir) * spaceMult + Time * timeMult);
    return wave;
}
VS_OUTPUT vs_RenderTerrain(VS_INPUT input)
{
    VS_OUTPUT output;
    //input.Position.y +=10;
    float4 worldPosition = mul(input.Position, World);
    float3 wave1 = createWave(4, 5, float2(0.5, 0.3), 40, 160, 3, 10, worldPosition);
    float3 wave2 = createWave(8, 5, float2(0.8, -0.4), 12, 120, 1.2, 20, worldPosition);
    float3 wave3 = createWave(4, 5, float2(0.3, 0.2), 2, 90, 5, 25, worldPosition);
    float3 wave4 = createWave(2, 5, float2(0.4, 0.25), 2, 60, 15, 15, worldPosition);
    float3 wave5 = createWave(6, 5, float2(0.1, 0.8), 20, 250, 2, 40, worldPosition);
    float3 wave6 = createWave(4, 5, float2(-0.5, -0.3), 0.5, 8, 0.2, 4, worldPosition);
    float3 wave7 = createWave(8, 5, float2(-0.8, 0.4), 0.3, 5, 0.3, 6, worldPosition);
    worldPosition.xyz += (wave1 + wave2 + wave3 + wave4 + wave5 + wave6 + wave7) / 7;
    float4 viewPosition = mul(worldPosition, View);
    
    float EPSILON = 0.001;
        float3 dxWave1 = createWave(4, 5, float2(0.5, 0.3), 40, 160, 3, 10, float4(worldPosition.x + EPSILON, worldPosition.yz, 1));
        float3 dzWave1 = createWave(4, 5, float2(0.5, 0.3), 40, 160, 3, 10, float4(worldPosition.xy, worldPosition.z + EPSILON, 1));
        float3 dxWave2 = createWave(8, 5, float2(0.8, -0.4), 12, 120, 1.2, 20, float4(worldPosition.x + EPSILON, worldPosition.yz, 1));
        float3 dzWave2 = createWave(8, 5, float2(0.8, -0.4), 12, 120, 1.2, 20, float4(worldPosition.xy, worldPosition.z + EPSILON, 1));
        float3 dxWave3 = createWave(4, 5, float2(0.3, 0.2), 2, 90, 5, 25, float4(worldPosition.x + EPSILON, worldPosition.yz, 1));
        float3 dzWave3 = createWave(4, 5, float2(0.3, 0.2), 2, 90, 5, 25, float4(worldPosition.xy, worldPosition.z + EPSILON, 1));
        float3 dxWave4 = createWave(2, 5, float2(0.4, 0.25), 2, 60, 15, 15, float4(worldPosition.x + EPSILON, worldPosition.yz, 1));
        float3 dzWave4 = createWave(2, 5, float2(0.4, 0.25), 2, 60, 15, 15, float4(worldPosition.xy, worldPosition.z + EPSILON, 1));
        float3 dxWave5 = createWave(6, 5, float2(0.1, 0.8), 20, 250, 2, 40, float4(worldPosition.x + EPSILON, worldPosition.yz, 1));
        float3 dzWave5 = createWave(6, 5, float2(0.1, 0.8), 20, 250, 2, 40, float4(worldPosition.xy, worldPosition.z + EPSILON, 1));
        float3 dxWave6 = createWave(4, 5, float2(-0.5, -0.3), 0.5, 8, 0.2, 4, float4(worldPosition.x + EPSILON, worldPosition.yz, 1));
        float3 dzWave6 = createWave(4, 5, float2(-0.5, -0.3), 0.5, 8, 0.2, 4, float4(worldPosition.xy, worldPosition.z + EPSILON, 1));
        float3 dxWave7 = createWave(8, 5, float2(-0.8, 0.4), 0.3, 5, 0.3, 6, float4(worldPosition.x + EPSILON, worldPosition.yz, 1));
        float3 dzWave7 = createWave(8, 5, float2(-0.8, 0.4), 0.3, 5, 0.3, 6, float4(worldPosition.xy, worldPosition.z + EPSILON, 1));
    float3 normalVector = float3(0, 0, 0);
        normalVector.x = (dxWave1.x + dxWave2.x + dxWave3.x + dxWave4.x + dxWave5.x + dxWave6.x + dxWave7.x) / 7;
        normalVector.z = (dxWave1.z + dxWave2.z + dxWave3.z + dxWave4.z + dxWave5.z + dxWave6.z + dxWave7.z) / 7;
    
        float3 waterTangent1 = normalize(float3(1, normalVector.x, 0));
        float3 waterTangent2 = normalize(float3(0, normalVector.z, 1));
        input.Normal.xyz = normalize(cross(waterTangent2, waterTangent1));
    output.WorldPosition = worldPosition;
    output.Normal = mul(float4(input.Normal, 1), InverseTransposeWorld);
    output.Position = mul(viewPosition, Projection);
    output.TextureCoordinates = input.TextureCoordinates;
    output.ScreenSpacePosition = mul(output.Position, WorldViewProjectionSun);
    output.LightSpacePosition = mul(output.WorldPosition, LightViewProjection);
    return output;
}
float3 getNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal = tex2D(normalSampler, textureCoordinates).xyz * 2.0 - 1.0;

    float3 Q1 = ddx(worldPosition);
    float3 Q2 = ddy(worldPosition);
    float2 st1 = ddx(textureCoordinates);
    float2 st2 = ddy(textureCoordinates);

    worldNormal = normalize(worldNormal.xyz);
    float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
    float3 B = -normalize(cross(worldNormal, T));
    float3x3 TBN = float3x3(T, B, worldNormal);

    return normalize(mul(tangentNormal, TBN));
}
float4 calcularSombra(float3 colorCalculado, VS_OUTPUT input){
    float3 lightSpacePosition = input.LightSpacePosition.xyz / input.LightSpacePosition.w;
    float2 shadowMapTextureCoordinates = 0.5 * lightSpacePosition.xy + float2(0.5, 0.5);
    shadowMapTextureCoordinates.y = 1.0f - shadowMapTextureCoordinates.y;
    float3 normal = normalize(input.Normal.rgb);
    float3 lightDirection = normalize(sunPosition - input.WorldPosition.xyz);
    float inclinationBias = max(modulatedEpsilon * (1.0 - dot(normal, lightDirection)), maxEpsilon);
    float notInShadow = 0.0;
    float2 texelSize = 1.0 / shadowMapSize;
    for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
        {
            float pcfDepth = tex2D(shadowMapSampler, shadowMapTextureCoordinates + float2(x, y) * texelSize).r + inclinationBias;
            notInShadow += step(lightSpacePosition.z, pcfDepth) / 9.0;
        }
	
    float4 baseColor = float4(colorCalculado,1);
    baseColor.rgb *= 0.5 + 0.5 * notInShadow;
    return baseColor;
}
float4 calcularRefleccion(float3 colorCalculado, VS_OUTPUT input){
    //Normalizar vectores
	float3 normal = normalize(input.Normal.xyz);
    
	
    // Not part of the mapping, just adjusting color
    float3 baseColor = lerp(colorCalculado, float3(1, 1, 1), step(length(colorCalculado), 0.01));
    
	//Obtener texel de CubeMap
	float3 view = normalize(cameraPosition.xyz - input.WorldPosition.xyz);
	float3 reflection = reflect(view, normal);
	float3 reflectionColor = texCUBE(environmentMapSampler, reflection).rgb;

    float fresnel = saturate((1.0 - dot(normal, view)));

    return float4(lerp(baseColor, reflectionColor, fresnel*0.25f), 1);
 }
float4 ps_RenderTerrain(VS_OUTPUT input) : COLOR0
{
    float alturaY = clamp(sunPosition.y / 1500, 0.5, 1);
    float3 normal = getNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, input.Normal.xyz);
    float3 worldNormal = input.Normal.xyz * normal;
    float3 reflNormal = input.Normal.xyz * normal;
    
    float3 lightDirection = normalize(sunPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(cameraPosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
    
    float4 texelColor = tex2D(textureSampler, input.TextureCoordinates);
    float4 foamColor = tex2D(foamSampler, input.TextureCoordinates);
    
    float3 reflColor = tex2D(textureSampler, input.TextureCoordinates).rgb;
    
    reflColor = lerp(reflColor, float3(1, 1, 1), step(length(reflColor), 0.01));
     
     float3 view = normalize(cameraPosition.xyz - input.WorldPosition.xyz);
     
     float3 reflection = reflect(view, reflNormal);
     
     //float3 reflectionColor = texCUBE(environmentMapSampler, reflection).rgb;
      
     float3 ambientLight = KAmbient * ambientColor + KFoam * foamColor.rgb;
    
     float NdotL = saturate(dot(worldNormal, lightDirection));
     
     float3 diffuseLight = KDiffuse * diffuseColor * NdotL;
    
     float3 baseColor = saturate(ambientLight + diffuseLight);
     
     float crestaBase = saturate(input.WorldPosition.y * 0.008) +0.22;
     baseColor += saturate(float3(1, 1, 1) * float3(crestaBase, crestaBase, crestaBase));
    /*
     if (input.WorldPosition.y * 0.1 > -1) {
            float n = input.WorldPosition.y * 0.5 * noise(input.WorldPosition.x * 0.01) * noise(input.WorldPosition.z * 0.01) * texelColor.r;
            baseColor += float3(.1, .1, .1) * float3(n * saturate(foamColor.r * 2), n * saturate(foamColor.r * 2), n * saturate(foamColor.r * 2));
     }
     return float4(saturate(baseColor.x), saturate(baseColor.y), saturate(baseColor.z),1);
     float3 specColor = specularColor * texelColor.r;
     float NdotH = dot(worldNormal, halfVector);
     float3 specularLight = sign(NdotL) * KSpecular * specColor * pow(saturate(NdotH), shininess);
     float4 finalColor = float4(baseColor + specularLight, 1) * alturaY;*/
    
     //return float4(finalColor.rgb, clamp((1 - foamColor.r), 0.95, 1));
     //return float4(baseColor,1);
    // return tex2D(colorMap, input.TextureCoordinates);
    float4 color_with_shadow = calcularSombra(baseColor, input);
    return calcularRefleccion(color_with_shadow, input);
    //return float4(baseColor,1);
}
DepthPassVertexShaderOutput DepthVS(in DepthPassVertexShaderInput input)
{
	DepthPassVertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
    float3 wave1 = createWave(4, 5, float2(0.5, 0.3), 40, 160, 3, 10, worldPosition);
    float3 wave2 = createWave(8, 5, float2(0.8, -0.4), 12, 120, 1.2, 20, worldPosition);
    float3 wave3 = createWave(4, 5, float2(0.3, 0.2), 2, 90, 5, 25, worldPosition);
    float3 wave4 = createWave(2, 5, float2(0.4, 0.25), 2, 60, 15, 15, worldPosition);
    float3 wave5 = createWave(6, 5, float2(0.1, 0.8), 20, 250, 2, 40, worldPosition);
    float3 wave6 = createWave(4, 5, float2(-0.5, -0.3), 0.5, 8, 0.2, 4, worldPosition);
    float3 wave7 = createWave(8, 5, float2(-0.8, 0.4), 0.3, 5, 0.3, 6, worldPosition);
    worldPosition.xyz += (wave1 + wave2 + wave3 + wave4 + wave5 + wave6 + wave7) / 7;
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.ScreenSpacePosition = mul(output.Position, WorldViewProjectionSun);
	return output;
}

float4 ps_RenderTerrainDepth(VS_OUTPUT input) : COLOR0
{
    float depth = input.ScreenSpacePosition.z / input.ScreenSpacePosition.w;
    return float4(1, 1, 1, 1);
}
technique DepthMap
{
    pass Pass_0
    {
        VertexShader = compile VS_SHADERMODEL DepthVS();
        PixelShader = compile PS_SHADERMODEL ps_RenderTerrainDepth();
    }
}

technique ShadowMap
{
    pass Pass_0
    {
        VertexShader = compile VS_SHADERMODEL vs_RenderTerrain();
        PixelShader = compile PS_SHADERMODEL ps_RenderTerrain();
    }
}
