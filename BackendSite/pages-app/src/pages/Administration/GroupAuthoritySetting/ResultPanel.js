import React from 'react';
import {Table, convertToTableResult, align, tableSize, tableType, fieldContent, tableColumnSize, tableClassName} from '../../../component/table'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'

const DefineTableHeaders=(props)=>{
    return [
        {DataColumn:'groupName', DisplayText:Lang.getLang(window.LangCode,"lbl_group_name"), ColumnSize:tableColumnSize.md, Align:align.center, BodyRowAlign:align.default},
        {DataColumn:'remark', DisplayText:Lang.getLang(window.LangCode,"lbl_remark"), Align:align.center, BodyRowAlign:align.default},
        {
            DataColumn:'Edit', DisplayText:Lang.getLang(window.LangCode,"lbl_edit"), ColumnSize:tableColumnSize.twoxs, Align:align.center,
            FieldContent:[{
                Type:fieldContent.EditIcon, 
                OnClick:props.ClickEditGroup
            }]
        },
        {
            DataColumn:'delete', DisplayText:Lang.getLang(window.LangCode,"lbl_delete"), ColumnSize:tableColumnSize.twoxs, Align:align.center,
            FieldContent:[{
                Type:fieldContent.DeleteIcon, 
                OnClick:props.ClickDeleteGroup
            }]
        }
    ];
}

export default (props)=> {
    const TableName = 'GroupList';
    const resultDatas = props.ResultDatas;
    const tableInfo = convertToTableResult(TableName, DefineTableHeaders(props), resultDatas, props.SortingInfo);
    return (
        <Table 
            TableClassName={tableClassName.main__row_table}
            TableName={tableInfo.TableName} 
            Header={tableInfo.Header} 
            Body={tableInfo.Body}            
            TableSize={tableSize.lg}
            TableType={tableType.default}
            NoDataDiplayText={Lang.getLang(window.LangCode,"lbl_no_data")}
        />
    );
}