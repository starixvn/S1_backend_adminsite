import {combineReducers} from 'redux'
import {TableInfoHandler} from './tableInfoHandler'
import {PopboxHandler} from './popboxHandler'
import { AddUserHandler } from './addUserHandler';
import {InitDataModel} from './actionsAndDataProperties'
import {UserAuthoritySettingHandler} from './userAuthoritySettingHandler'
import {AssignIndividualPermissionHandler} from './assignIndividualPermissionHandler'
import {AssignGroupHandler} from './assignGroupHandler'
import {AssignCurrencyHandler} from './assignCurrencyHandler'
import {ResetPasswordHandler} from './resetPasswordHandler'

if(process.env.NODE_ENV!=='production'){
    window.SubsidiaryList = {"ha":"ha","hp":"hp","itgo":"itgo","qhc":"qhc","tb":"tb","zy":"zy","TestSub001":"TestSub001","Scott":"Scott","Ariel":"Ariel","Martin":"Martin","sunriseB2B":"sunriseB2B","tes01":"tes01","TestSub002":"TestSub002","wfefew":"wfefew","    ":"    ","ariel 88":"ariel 88","ariel88":"ariel88","test":"test","*Bet 168":"*Bet 168","test02":"test02","TestSub003":"TestSub003","TestSub004":"TestSub004","TestSub005":"TestSub005","TestSub006":"TestSub006","TestSub007":"TestSub007","TestSub008":"TestSub008","TestSub009":"TestSub009","TestSub010":"TestSub010","?%^&*()_-":"?%^&*()_-","TestSub0012":"TestSub0012","TestSub0011":"TestSub0011","ariel881":"ariel881","arieltets":"arieltets","testsub013":"testsub013","licTest":"licTest","190814test":"190814test"};
}

const initPopboxInfo = {
    PopboxName:null,
    Title:null,
    Content:null,
    ExecutingStatus:null,
    IsOpen:false,
    IdentityKey:null
}

const initCommonModel = {
    Datas:{
        UserList:null,
        UserPermissions:InitDataModel.UserPermissions
    },
    AddUserPopbox:Object.assign({}, InitDataModel.AddUserDataModel, {
        ...InitDataModel.AddUserDataModel
    }),
    AssignGroupPopbox:{
        ...InitDataModel.AssignGroupDataModel
    },
    AssignCurrencyPopbox:{
        ...InitDataModel.AssignCurrencyDataModel
    },
    ResetPasswordPopbox:{
        ...InitDataModel.ResetPasswordDataModel
    },
    PopboxInfo:initPopboxInfo
}

export const DefineHandlers = {
    'TableInfoHandler':'TableInfoHandler',
    'PopboxHandler':'PopboxHandler',
    'AddUserHandler':'AddUserHandler',
    'UserAuthoritySettingHandler':'UserAuthoritySettingHandler',
    'AssignIndividualPermissionHandler':'AssignIndividualPermissionHandler',
    'AssignGroupHandler':'AssignGroupHandler',
    'AssignCurrencyHandler':'AssignCurrencyHandler',
    'ResetPasswordHandler':'ResetPasswordHandler'
}

function CombineHandler(state=initCommonModel, action){
    switch(action.handler){
        case DefineHandlers.TableInfoHandler:
            return TableInfoHandler(state, action)
        case DefineHandlers.PopboxHandler:
            return PopboxHandler(state, action)
        case DefineHandlers.AddUserHandler:
            return AddUserHandler(state, action)
        case DefineHandlers.UserAuthoritySettingHandler:
            return UserAuthoritySettingHandler(state, action)
        case DefineHandlers.AssignIndividualPermissionHandler:
            return AssignIndividualPermissionHandler(state, action)
        case DefineHandlers.AssignGroupHandler:
            return AssignGroupHandler(state, action)
        case DefineHandlers.AssignCurrencyHandler:
            return AssignCurrencyHandler(state, action)
        case DefineHandlers.ResetPasswordHandler:
            return ResetPasswordHandler(state, action)
        default:
            return initCommonModel;
    }
}

const Reducer = combineReducers({
    CommonHandler: CombineHandler
})

export default Reducer