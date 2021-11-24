#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float KAmbient;
float3 ambientColor;

float KDiffuse;
float3 diffuseColor;

float KSpecular;
float3 specularColor;
float shininess;

float KReflection;
float KFoam;

texture texColorMap;
sampler2D colorMap = sampler_state
{
    Texture = (texColorMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

struct VS_INPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 TextureCoordinates : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
};

VS_OUTPUT vs_RenderTerrain(VS_INPUT input)
{
    VS_OUTPUT output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.WorldPosition = worldPosition;
    output.Normal = input.Normal;
    output.Texcoord = input.Texcoord;
    return output;
}


float4 ps_RenderTerrain(VS_OUTPUT input) : COLOR0
{
    float alturaY = clamp(sunPosition.y / 1500, 0.5, 1);
    
        float3 normal = getNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, input.Normal.xyz);
    
        //float3 worldNormal = input.Normal.xyz * normal;
        float3 worldNormal = input.Normal.xyz + normal;
        float3 reflNormal = input.Normal.xyz * normal;
    
    
        // Base vectors
        float3 lightDirection = normalize(sunPosition - input.WorldPosition.xyz);
        float3 viewDirection = normalize(cameraPosition - input.WorldPosition.xyz);
        float3 halfVector = normalize(lightDirection + viewDirection);
    
        // Get the texture texel textureSampler is the sampler, Texcoord is the interpolated coordinates
        float4 texelColor = tex2D(textureSampler, input.TextureCoordinates);
        float4 foamColor = tex2D(foamSampler, input.TextureCoordinates);
    
        // Get the texel from the texture
        float3 reflColor = tex2D(textureSampler, input.TextureCoordinates).rgb;
    
        // Not part of the mapping, just adjusting color
        reflColor = lerp(reflColor, float3(1, 1, 1), step(length(reflColor), 0.01));
    
        float3 view = normalize(cameraPosition.xyz - input.WorldPosition.xyz);
        float3 reflection = reflect(view, reflNormal);
        float3 reflectionColor = texCUBE(environmentMapSampler, reflection).rgb;
    
        float3 ambientLight = KAmbient * ambientColor + KFoam * foamColor.rgb;
    
        // Calculate the diffuse light
        float NdotL = saturate(dot(worldNormal, lightDirection));
        float3 diffuseLight = KDiffuse * diffuseColor * NdotL;
    
        float3 baseColor = saturate(ambientLight + diffuseLight);
    
        float crestaBase = saturate(input.WorldPosition.y * 0.008) + 0.22;
        baseColor += saturate(float3(1, 1, 1) * float3(crestaBase, crestaBase, crestaBase));
    
        if (input.WorldPosition.y * 0.1 > -1) {
            float n = input.WorldPosition.y * 0.5 * noise(input.WorldPosition.x * 0.01) * noise(input.WorldPosition.z * 0.01) * texelColor.r;
            baseColor += float3(.1, .1, .1) * float3(n * saturate(foamColor.r * 2), n * saturate(foamColor.r * 2), n * saturate(foamColor.r * 2));
        }
    
        float3 specColor = specularColor * texelColor.r;
        float NdotH = dot(worldNormal, halfVector);
        float3 specularLight = sign(NdotL) * KSpecular * specColor * pow(saturate(NdotH), shininess);
        float4 finalColor = float4(lerp(baseColor, reflectionColor * KReflection, 0.5) + specularLight, 1) * alturaY;
    
        //return float4(finalColor.rgb, clamp((1 - foamColor.r), 0.95, 1));
        return float4(baseColor,1);
    //return tex2D(colorMap, normalize(input.Texcoord.xy));
}

technique RenderTerrain
{
    pass Pass_0
    {
        VertexShader = compile VS_SHADERMODEL vs_RenderTerrain();
        PixelShader = compile PS_SHADERMODEL ps_RenderTerrain();
    }
}
