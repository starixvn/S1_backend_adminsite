export const RuleType = {
    'NotAllowNull':'NotAllowNull',
    'NotAllowWhiteSpace':'NotAllowWhiteSpace',
    'MaxLength':'MaxLength',
    'MinLength':'MinLength',
    'Regex':'Regex'
}

export function VaildFieldValues(datas, defineElements){
    let checked = true;
    for(let key in defineElements){
        let elem = defineElements[key]
        if(elem.Rule){
            if(!VaildFieldValue(datas, datas[elem.DataColumn], elem.Rule, elem.PrefixText)){
                checked = false;
                break;
            }
        }        
    }

    return checked;
}

export function VaildFieldValue(datas, value, rule, prefixTextRule){
    let matchRule = true;
    let alertMsg = '';
    for(let key in rule){
        if(rule[key].AllowNull && !value) continue;
        switch(key){
            case RuleType.NotAllowNull:
                    if(rule[key].WhenValueIs){
                        if(datas[rule[key].WhenValueIs.DataColumn]===rule[key].WhenValueIs.Value || rule[key].WhenValueIs.HasValue){
                            matchRule = value ? true : false;    
                        }
                    }else{
                        matchRule = value ? true : false;
                    }
                break;

            case RuleType.NotAllowWhiteSpace:
                    matchRule = value.indexOf(' ') === -1
                break;
            
            case RuleType.MaxLength:
                    if(prefixTextRule && prefixTextRule.DisplayWhen && datas[prefixTextRule.DisplayWhen.DataColumn] === prefixTextRule.DisplayWhen.WhenDataColumnValueIs){
                        value = datas[prefixTextRule.DataColumn]+value;
                    }
                    matchRule = rule[key].Value >= value.length
                break;
            
            case RuleType.MinLength:
                    matchRule = rule[key].Value <= value.length
                break;

            case RuleType.Regex:
                    matchRule = value.match(rule[key].Value)
                break;
            
            default:
                break;
        }

        if(!matchRule){
            alertMsg = rule[key].AlertMsg
            break;
        }
    }

    if(alertMsg) alert(alertMsg);
    return matchRule;
}

export function CheckMinMaxValue(minValue, maxValue, alertMsg){
    let result = true
    let regex = /^[0-9]+\.?[0-9]*$/
    if((typeof(minValue) === 'string' && !minValue.match(regex)) || (typeof(maxValue) === 'string' && !maxValue.match(regex))){
        result = false;
        alert(alertMsg)
    }else if(minValue > maxValue){
        result = false;
        alert(alertMsg)
    }

    return result;
}