import React from 'react';
import {Table, convertToTableResult, align, tableSize, tableType, fieldContent, tableColumnSize, tableClassName} from '../../../component/table'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'

function DefineTableHeaders(props){
    let result =  [
        {DataColumn:'UserID', DisplayText:Lang.getLang(window.LangCode,"lbl_UserID"), ColumnSize:tableColumnSize.xs, Align:align.center, Rowspan:2, HasSort:true},
        {DataColumn:'UserName', DisplayText:Lang.getLang(window.LangCode,"lbl_UserName"), ColumnSize:tableColumnSize.xs, Align:align.center, Rowspan:2, HasSort:true},
        {
            DataColumn:'Enabled', DisplayText:Lang.getLang(window.LangCode,"lbl_enable"), ColumnSize:tableColumnSize.xs, Align:align.center, Rowspan:2, HasSort:true,
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
                    DataColumn:'UserID',
                    ValueIs:'admin'
                }
            }
        },
        {
            DataColumn:'Permission', DisplayText:Lang.getLang(window.LangCode,"lbl_permission"), Align:align.center, Colspan:3,
            SubRows:[
                {
                    DataColumn:'GroupName', DisplayText:Lang.getLang(window.LangCode,"lbl_role"), ColumnSize:tableColumnSize.xs, Align:align.center, BodyRowAlign:align.default, HasSort:true,
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
                            DataColumn:'UserID',
                            ValueIs:'admin'
                        }
                    }
                },
                {
                    DataColumn:'Individual', DisplayText:Lang.getLang(window.LangCode,"lbl_individual"), ColumnSize:tableColumnSize.twosm, Align:align.center,
                    FieldContent:[{
                        Type:fieldContent.UserIcon,
                        OnClick:props.ClickAssignUserPermission
                    }],
                    SetColumnToEmpty:{
                        When:{
                            DataColumn:'UserID',
                            ValueIs:'admin'
                        }
                    }
                },
                {
                    DataColumn:'Currency', DisplayText:Lang.getLang(window.LangCode,"lbl_currency"), ColumnSize:tableColumnSize.twosm, Align:align.center,
                    FieldContent:[{Type:fieldContent.EditIcon,
                        OnClick:props.ClickAssignCurrency
                    }],
                    SetColumnToEmpty:{
                        When:{
                            DataColumn:'UserID',
                            ValueIs:'admin'
                        }
                    }
                }
            ]
        },
        {
            DataColumn:'Password', DisplayText:Lang.getLang(window.LangCode,"lbl_password"), ColumnSize:tableColumnSize.min2, Align:align.center, Rowspan:2,
            FieldContent:[{Type:fieldContent.KeyIcon, OnClick:props.ClickResetPassword}]
        },
        {
            DataColumn:'Delete', DisplayText:Lang.getLang(window.LangCode,"lbl_delete"), ColumnSize:tableColumnSize.twoxs, Align:align.center, Rowspan:2,
            FieldContent:[{
                Type:fieldContent.DeleteIcon,
                OnClick:props.ClickDeleteUser
            }],
            SetColumnToEmpty:{
                When:{
                    DataColumn:'UserID',
                    ValueIs:'admin'
                }
            }
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