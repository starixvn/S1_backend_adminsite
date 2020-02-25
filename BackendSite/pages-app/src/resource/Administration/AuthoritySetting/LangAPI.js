import {cs} from './resource.cs'
import {en} from './resource.en'

export default class lang{
    static getLang(LangCode, resKey, value = []){
        let getResult = null;
        switch (LangCode) {
            case "cs":
                getResult = cs[resKey];
                break;
        
            case "en":
                getResult = en[resKey]
                break;
            default:
                getResult = en[resKey]
        }

        value && value.forEach((v, idx)=>{
            getResult = getResult.replace(`{${idx}}`, v);
        });

        return getResult ? getResult : resKey
    }
}