import React, { Component } from 'react'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'
import { BtnBarWithRightAndLeftGroup, Button, buttonClass, buttonSize } from '../../../component/buttons'
import Permissions from '../component/permissionList'
import $ from 'jquery'


export class IndividualPermissionPopboxContent extends Component {
    constructor(props){
        super(props);
        this.popup_body = React.createRef();
    }
    componentDidMount=()=>{
        if(process.env.NODE_ENV === 'production'){
            $(this.refs.popup_body).overlayScrollbars({});
        }
    }
    render() {
        return (
            <div className="popup__body" ref="popup_body">
                <Notice />
                <Permissions
                    Datas={this.props.Datas.PermissionDatas}
                    Functions={
                        {
                            HandleList:this.props.Datas.HandleList,
                            EnableOrDisableFuncItem:this.props.EnableOrDisableFuncItem,
                            EnableOrDisableSubOptionsItem:this.props.EnableOrDisableSubOptionsItem,
                            OpenOrCloseList:this.props.OpenOrCloseList
                        }
                    }
                    IsUserAuthPage={true}
                    IsFullPage={false}
                />
                <div className="popup__row">
                    <IndividualPermissionBtnBar
                        ResetToGroupSetting={this.props.ResetToGroupSetting}
                        SaveUserPermission={this.props.SaveUserPermission}
                        Close={this.props.ClosePopbox}
                    />
                </div>
            </div>
        )
    }
}

const IndividualPermissionBtnBar = (props)=>{
    let leftBtns = [
        <Button key={'individual_permission_btn_reset_to_group_setting'}
            DisplayText={Lang.getLang(window.LangCode, "lbl_reset_to_group_setting")}
            ToUpperCase={true}
            DisplayTextIsElem={false}
            ButtonClass={buttonClass.newbtnTertiary}
            OnClick={props.ResetToGroupSetting}
        /> 
    ];
    
    let rightBtns = [
        <Button key={'individual_permission_btn_save'}
            DisplayText={Lang.getLang(window.LangCode, "lbl_save")}
            ToUpperCase={true}
            DisplayTextIsElem={false}
            ButtonClass={buttonClass.newbtnPrimary}
            ButtonSize={buttonSize.min}
            OnClick={props.SaveUserPermission}
        />,
        <Button key={'individual_permission_btn_cancel'}
            DisplayText={Lang.getLang(window.LangCode, "lbl_Cancel")}
            ToUpperCase={true}
            DisplayTextIsElem={false}
            ButtonClass={buttonClass.newbtnSecondary}
            ButtonSize={buttonSize.min}
            OnClick={props.Close}
        />
    ];

    return(
        <BtnBarWithRightAndLeftGroup 
            LeftButtons={leftBtns}
            RightButtons={rightBtns}
            
        />
    );
}

const Notice = ()=>{
    return (
        <div className="popup__row">
            <i className="icon icon-error text-highlight"></i>
            <span className="text text-highlight">{Lang.getLang(window.LangCode, "msg_UserPermission_Notice")}</span>
        </div>
    );
}