import React, {Component} from 'react'
import ReactDOM from 'react-dom'
import { reducerActions } from './redux/actionsAndDataProperties'
import {store, mapStateToProps, mapDispatchToProps, } from './redux/store'
import { Provider, connect } from 'react-redux'
import ResultTable from './ResultPanel'
import EditPanel from './EditPanel'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'
import {Button, buttonClass} from '../../../component/buttons'

class Panel extends Component{
    constructor(props){
        super(props);
        this.props[reducerActions.GroupAuthorityActionTypes.GET_GROUP_LIST]();
    }
    get SubmitFunction(){
        switch(this.props.Edit.ActionType){
            case reducerActions.GroupAuthorityActionTypes.CLICK_ADD_GROUP:
                return this.props[reducerActions.GroupAuthorityActionTypes.ADD_GROUP]
            case reducerActions.GroupAuthorityActionTypes.CLICK_EDIT_GROUP:
                    return this.props[reducerActions.GroupAuthorityActionTypes.EDIT_GROUP]
            default:
                return this.props[reducerActions.GroupAuthorityActionTypes.ADD_GROUP]
        }
    }
    render(){
        return (
            <div className="newwrapper">
                {
                    this.props.Edit.ShowPanel ? (
                    <EditPanel
                        GroupInfo={
                            {
                                IsAdd:this.props.Edit.ActionType===reducerActions.GroupAuthorityActionTypes.CLICK_ADD_GROUP,
                                Datas:this.props.Edit.GroupInfo,
                                EditField:this.props[reducerActions.GroupAuthorityActionTypes.EDIT_GROUP_INFO_FIELD]
                            }
                        }
                        Datas={this.props.Datas.Permissions}
                        EnableOrDisableFuncItem={this.props[reducerActions.PermissionListActionTypes.ENABLE_OR_DISABLE_FUNCTIONITEM]}
                        EnableOrDisableSubOptionsItem={this.props[reducerActions.PermissionListActionTypes.ENABLE_OR_DISABLE_SUBOPTION]}
                        OpenOrCloseList={this.props[reducerActions.PermissionListActionTypes.OPEN_OR_CLOSE_MAIN_LIST]}
                        BtnBar={{
                            ClickCancel:this.props[reducerActions.GroupAuthorityActionTypes.CLOSE_EDIT_PANEL],
                            ClickSubmit:this.SubmitFunction
                        }}
                    />) : (<GroupListPanel {...this.props} />)
                }
            </div>
        )
    }
}

const GroupListPanel = (props)=>{
    return(
        props.Datas.GroupList === null || props.Datas.GroupList === undefined ? <LoadingImage />  :
        <>
            <Header ClickAddGroup={props[reducerActions.GroupAuthorityActionTypes.CLICK_ADD_GROUP]}/>
            <div className="main">
                <ResultTable
                    ResultDatas={props.Datas.GroupList}
                    ClickEditGroup={props[reducerActions.GroupAuthorityActionTypes.CLICK_EDIT_GROUP]}
                    ClickDeleteGroup={props[reducerActions.GroupAuthorityActionTypes.CLICK_DELETE_GROUP]}
                    ClickSorting={props[reducerActions.GroupAuthorityActionTypes.CLICK_SORTING]}
                    SortingInfo={props.Datas.SortingInfo}
                />
            </div>
        </>
    )
}

const Header = (props)=>{
    return(
        <div className="header">
            <Button ButtonClass={buttonClass.newbtnAdd} DisplayText={Lang.getLang(window.LangCode, "lbl_add_user_group")} OnClick={props.ClickAddGroup}/>
        </div>
    )
}

const LoadingImage = ()=>{
    return (
        <div className="loading-process">
            <svg className="icon-loading" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32" width="64" height="64" fill="#4C6375">
                <circle transform="translate(8 0)" cx="0" cy="16" r="0">
                    <animate attributeName="r" values="0; 4; 0; 0" dur="1.2s" repeatCount="indefinite" begin="0"
                                keyTimes="0;0.2;0.7;1" keySplines="0.2 0.2 0.4 0.8;0.2 0.6 0.4 0.8;0.2 0.6 0.4 0.8" calcMode="spline" />
                </circle>
                <circle transform="translate(16 0)" cx="0" cy="16" r="0">
                    <animate attributeName="r" values="0; 4; 0; 0" dur="1.2s" repeatCount="indefinite" begin="0.3"
                                keyTimes="0;0.2;0.7;1" keySplines="0.2 0.2 0.4 0.8;0.2 0.6 0.4 0.8;0.2 0.6 0.4 0.8" calcMode="spline" />
                </circle>
                <circle transform="translate(24 0)" cx="0" cy="16" r="0">
                    <animate attributeName="r" values="0; 4; 0; 0" dur="1.2s" repeatCount="indefinite" begin="0.6"
                                keyTimes="0;0.2;0.7;1" keySplines="0.2 0.2 0.4 0.8;0.2 0.6 0.4 0.8;0.2 0.6 0.4 0.8" calcMode="spline" />
                </circle>
            </svg>
        </div>
    );    
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