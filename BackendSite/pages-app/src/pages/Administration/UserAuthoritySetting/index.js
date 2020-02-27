import React, {Component} from 'react'
import ReactDOM from 'react-dom'
import { reducerPropDefines, reducerActions } from './redux/actionsAndDataProperties'
import {store, mapStateToProps, mapDispatchToProps, } from './redux/store'
import { Provider, connect } from 'react-redux'
import {Popbox} from '../../../component/popbox'
import ResultTable from './ResultPanel'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'
import PopboxContent from './PopboxContent'
import {Button, buttonClass} from '../../../component/buttons'
import LoadingImage from '../../../component/preloader'

class Panel extends Component{
    constructor(props){
        super(props);
        this.props[reducerActions.UserAuthoritySettingActionTypes.GET_USER_LIST]();
    }
    getPopboxCloseFunc(){
        let popboxName = this.props.PopboxInfo.PopboxName
        switch(popboxName){
            case reducerPropDefines.PopboxNames.AddUser:
                return this.props[reducerActions.AddUserActionTypes.CLOSE_ADD_USER_POPBOX]                
            case reducerPropDefines.PopboxNames.AssignGroup:
                return this.props[reducerActions.AssignGroupActionTypes.CLOSE_POPUP_ASSIGN_GROUP]
            case reducerPropDefines.PopboxNames.AssignSites:
                return this.props[reducerActions.AssignSitesActionTypes.CLOSE_POPUP_ASSIGN_SITES]
            case reducerPropDefines.PopboxNames.ResetPassword:
                return this.props[reducerActions.ResetPasswordActionTypes.CLOSE_POPUP_RESET_PASSWORD]
            default:
                return this.props[reducerActions.PopboxActionTypes.CLOSE_POPBOX]
        }
    }
    render(){
        return (
            this.props.Datas.UserList === null || this.props.Datas.UserList === undefined ? <LoadingImage />  : (
                <div className="newwrapper">
                    <Popbox
                        Title={this.props.PopboxInfo.Title}
                        Content={<PopboxContent {...this.props}/>}
                        ClickClose={this.getPopboxCloseFunc()}
                        IsOpen={this.props.PopboxInfo.IsOpen}
                    />
                    <Header OpenAddUserPanel={this.props[reducerActions.AddUserActionTypes.OPEN_ADD_USER_POPBOX]}/>
                    <Main {...this.props}/>
                </div>
            )
        )
    }
}

const Header = (props)=>{
    return(
        <div className="header">
            <Button ButtonClass={buttonClass.newbtnAdd} DisplayTextIsElem={false} DisplayText={Lang.getLang(window.LangCode, "lbl_add_user")} OnClick={props.OpenAddUserPanel}/>
        </div>
    )    
}

const Main = (props)=>{
    return(
        <div className="main">
            <ResultTable
                ResultDatas={props.Datas.UserList}
                ClickEnableUser={props[reducerActions.UserAuthoritySettingActionTypes.CLICK_ENABLE_USER]}
                ClickAssignGroup={props[reducerActions.AssignGroupActionTypes.OPEN_POPUP_ASSIGN_GROUP]}
                ClickResetPassword={props[reducerActions.ResetPasswordActionTypes.OPEN_POPUP_RESET_PASSWORD]}
                ClickAssignSites={props[reducerActions.AssignSitesActionTypes.OPEN_POPUP_ASSIGN_SITES]}
                ClickDeleteUser={props[reducerActions.UserAuthoritySettingActionTypes.CLICK_DELETE_USER]}
            />
        </div>
    )
}

const AppWithState = connect(mapStateToProps, mapDispatchToProps)(Panel)

export default function App(){
    return(
        <Provider store={store}>
            <AppWithState />
        </Provider>
    )    
}

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}