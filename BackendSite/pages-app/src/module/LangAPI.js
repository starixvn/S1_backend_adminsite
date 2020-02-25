import {cs} from '../resource/resource.cs'
import {en} from '../resource/resource.en'

export default class lang{
    static getLang(LangCode, resKey){
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

        return getResult ? getResult : resKey
    }
}