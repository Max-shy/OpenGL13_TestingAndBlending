#version 330 core
out vec4 FragColor;

in vec3 FragPos;//����ռ�����
in vec3 Normal;//������
in vec2 TexCoords;

//�����Դ
struct DirLight {
    vec3 direction;//����ⷽ��

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};  

//���Դ
struct PointLight {
    vec3 position;

    float constant;
    float linear;
    float quadratic;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};  

//�۹��
struct SpotLight{
    vec3 position;
    vec3 direction;

    float cutOff;
    float outerCutOff;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);//���㶨���Ӱ��
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);//������ԴӰ��
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);//����۹�ԴӰ��

uniform sampler2D texture_specular1;
uniform sampler2D texture_diffuse1;

uniform float mixValue;
uniform vec3 viewPos;//���λ��
uniform vec3 lightColor;

uniform DirLight dirLight;
uniform SpotLight spotLight;
uniform PointLight pointLight;//���Դ

void main()
{    

    //��һ������
    vec3 norm = normalize(Normal);//��������׼��
    vec3 viewDir = normalize(viewPos - FragPos);//���߷���
    
    //���㶨���Ӱ��
    vec3 result = CalcDirLight(dirLight, norm, viewDir);

    //������ԴӰ��
    result += CalcPointLight(pointLight, norm, FragPos, viewDir);    
       
    //����۹�Ӱ��
    result += CalcSpotLight(spotLight, norm, FragPos, viewDir);

    //��ɫ���(���Դ+�����+�۹�)
    FragColor = vec4(result,1.0);
    //FragColor = texture(texture_diffuse1, TexCoords) + texture(texture_specular1, TexCoords);
}





vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir){
    vec3 lightDir = normalize(-light.direction);
   // ��������ɫ
    float diff = max(dot(normal, lightDir), 0.0);
   // �������ɫ
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
   // �ϲ����
    vec3 ambient  = light.ambient  * vec3(texture(texture_diffuse1, TexCoords));
    vec3 diffuse  = light.diffuse  * diff * vec3(texture(texture_diffuse1, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(texture_specular1, TexCoords));
    return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    // ��������ɫ
    float diff = max(dot(normal, lightDir), 0.0);
    // �������ɫ
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    // ˥��
    float distance    = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    // �ϲ����
    vec3 amdwbient  = light.ambient  * vec3(texture(texture_diffuse1, TexCoords));
    vec3 diffuse  = light.diffuse  * diff * vec3(texture(texture_diffuse1, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(texture_specular1, TexCoords));
    
    ambient  *= attenuation;
    diffuse  *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(SpotLight light, vec3 norm, vec3 fragPos, vec3 viewDir)
{
     
     vec3 lightDir = normalize(light.position - FragPos);
     float diff = max(dot(norm, lightDir), 0.0);
     
     // specular
     vec3 reflectDir = reflect(-lightDir, norm);  
     float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);

     // attenuation
     float distance    = length(light.position - FragPos);
     float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
     // spotlight (soft edges)
     float theta = dot(lightDir, normalize(-light.direction)); 
     float epsilon = (light.cutOff - light.outerCutOff);
     float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

     vec3 ambient  = light.ambient  * vec3(texture(texture_diffuse1, TexCoords));
     vec3 diffuse  = light.diffuse  * diff * vec3(texture(texture_diffuse1, TexCoords));
     vec3 specular = light.specular * spec * vec3(texture(texture_specular1, TexCoords));

    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;

     return (ambient + diffuse + specular);
}