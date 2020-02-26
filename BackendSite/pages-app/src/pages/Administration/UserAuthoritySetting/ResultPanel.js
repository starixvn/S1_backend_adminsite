import React from 'react';
import {Table, convertToTableResult, align, tableSize, tableType, fieldContent, tableColumnSize, tableClassName} from '../../../component/table'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'

function DefineTableHeaders(props){
    let result =  [
        {DataColumn:'isSuper', HideColumn:true},
        {DataColumn:'userName', DisplayText:Lang.getLang(window.LangCode,"lbl_UserName"), ColumnSize:tableColumnSize.lg, Align:align.center, Rowspan:2},
        {
            DataColumn:'status', DisplayText:Lang.getLang(window.LangCode,"lbl_enable"), ColumnSize:tableColumnSize.xs, Align:align.center, Rowspan:2,
            FieldContent:[{
                Type:fieldContent.Checkbox,
                ConfirmAlert:{
                    CheckedAlertText:Lang.getLang(window.LangCode,"msg_disable_user"),
                    UnCheckedAlertText:Lang.getLang(window.LangCode,"msg_enable_user")
                },
                OnClick:props.ClickEnableUser
            }],
            SetColumnToEmpty:{
                When:{
                    DataColumn:'isSuper',
                    ValueIs:true
                }
            }
        },
        {
            DataColumn:'Permission', DisplayText:Lang.getLang(window.LangCode,"lbl_permission"), Align:align.center, Colspan:2,
            SubRows:[
                {
                    DataColumn:'groupName', DisplayText:Lang.getLang(window.LangCode,"lbl_role"), ColumnSize:tableColumnSize.xs, Align:align.center, BodyRowAlign:align.default,
                    FieldContent:[{
                        Type:fieldContent.EditIcon, 
                        OnClick:props.ClickAssignGroup, 
                        DisplayCurrentValue:true,
                        WhenNull:{
                            NullType:fieldContent.AddIcon,
                            NullDisplayText:Lang.getLang(window.LangCode,"lbl_add_role")
                        }
                    }],
                    SetColumnToEmpty:{
                        When:{
                            DataColumn:'isSuper',
                            ValueIs:true
                        }
                    }
                },
                {
                    DataColumn:'site', DisplayText:Lang.getLang(window.LangCode,"lbl_site"), ColumnSize:tableColumnSize.xs, Align:align.center,
                    FieldContent:[{Type:fieldContent.EditIcon,
                        OnClick:props.ClickAssignSites
                    }],
                    SetColumnToEmpty:{
                        When:{
                            DataColumn:'isSuper',
                            ValueIs:true
                        }
                    }
                }
            ]
        },
        {
            DataColumn:'Password', DisplayText:Lang.getLang(window.LangCode,"lbl_password"), ColumnSize:tableColumnSize.xs, Align:align.center, Rowspan:2,
            FieldContent:[{Type:fieldContent.KeyIcon, OnClick:props.ClickResetPassword}]
        }
    ]

    return result;
}

export default (props)=>{
    const TableName = 'UserList';
    const resultDatas = props.ResultDatas;
    const tableInfo = convertToTableResult(TableName, DefineTableHeaders(props), resultDatas);
    return (    
        <Table
            TableName={tableInfo.TableName}
            Header={tableInfo.Header} 
            Body={tableInfo.Body}
            TableSize={tableSize.lg}
            TableType={tableType.mergeCell}
            NoDataDiplayText={Lang.getLang(window.LangCode,"lbl_no_data")}
            TableClassName={tableClassName.main__row_table}
        />
    );
}