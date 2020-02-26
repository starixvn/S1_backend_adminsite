import React, { Component } from 'react';
import {Span} from './span'
import {InputBox} from './inputField'
import {DropdownBox} from './dropdownBox'
import DateFormat from '../module/Formatting/DateFormat'
import TimeZone from '../module/TimeZone'
import {CheckBox} from './checkBox'
import $ from 'jquery'

export const align = {
    center:'td--center',
    right:'td--right',
    default:''
}

export const tableClassName = {
    'panel__row_table':'panel__row-table',
    'popup__row_table':'popup__row-table',
    'main__row_table':'main__row-table'
}

export const tableOrder = {
    default:'table-default',
    primary:'table-primary'
}

export const tableSortingStatus = {
    desc:'is-decrease',
    asc:'is-increase',
    default:''
}

export const tableSize = {
    lg:'table-container--lg',
    md:'table-container--md',
    xs:'table-container--xs'
}

export const tableColumnSize = {
    xs:"td-xs",
    sm:"td-sm",
    md:"td-md",
    lg:'td-lg',
    min:'td-min',
    twoxs:'td-2xs',
    twosm:'td-2sm',
    min2:'td-min2',
    default:''
}

export const tableType = {
    mergeCell:'table--mergecell',
    withoutTableBlock:'',
    default:'table-block'
}

export const headerContent = {
    Checkbox:'Checkbox'
}

export const fieldContent = {
    Checkbox:'Checkbox',
    EditIcon:'EditIcon',
    DeleteIcon:'DeleteIcon',
    AddIcon:'AddIcon',
    UserIcon:'UserIcon',
    KeyIcon:'KeyIcon',
    HistoryIcon:'HistoryIcon'
}

const fieldContentType = {
    Icon:'Icon',
    Checkbox:'Checkbox'
}

const filedContentTypeAndClass = {
    EditIcon:{Type:fieldContentType.Icon, ClassName:'icon-mode_edit'},
    DeleteIcon:{Type:fieldContentType.Icon, ClassName:'icon-delete'},
    AddIcon:{Type:fieldContentType.Icon, ClassName:'icon-add'},
    UserIcon:{Type:fieldContentType.Icon, ClassName:'icon-supervisor_account'},
    KeyIcon:{Type:fieldContentType.Icon, ClassName:'icon-vpn_key'},
    HistoryIcon:{Type:fieldContentType.Icon, ClassName:'icon-clipboard-text'},
    Checkbox:{Type:fieldContentType.Checkbox}
}

export const fieldFormat = {
    DateTime:'DateTime',
    SequenceNumber:'Seq',
    DecimalRoundDown:'DecimalRoundDown'
}

export const tableBlockContentType = {
    span:'span',
    inputBox:'inputBox',
    dropdownBox:'dropdownBox'
}

export const TableBlock = (props)=>{
    let contentRows = props.BlockRowsDefine && props.BlockRowsDefine.Content.map((tr, trIdx)=>{
        return <tr key={`${props.BlockRowsDefine.BlockName}-${trIdx}`} className="tr">{tr.map((td, tdIdx)=>{
            let _content = null
            let key = `${props.BlockRowsDefine.BlockName}-${trIdx}-${tdIdx}`
            switch(td.ContentType){
                case tableBlockContentType.span:
                    _content = <Span DisplayText={td.Value} />
                    break
                case tableBlockContentType.inputBox:
                    _content = <InputBox MainKey={key} Value={td.InputBoxProps.Value} OnBlur={td.InputBoxProps.OnBlur} IsEdited={td.InputBoxProps.IsEdited} ReadOnly={props.IsReadOnly}/>
                    break
                case tableBlockContentType.dropdownBox:
                    _content = <DropdownBox
                                    SelectValue={td.DropdownBoxProps.SelectValue}
                                    Items={td.DropdownBoxProps.Items} 
                                    DropDownBoxClassName={td.DropdownBoxProps.DropDownBoxClassName} 
                                    OnSelectItem={td.DropdownBoxProps.OnSelectItem}
                                    HasTitle={td.DropdownBoxProps.HasTitle}
                                    MainKey={key}
                                    IsEdited={td.DropdownBoxProps.IsEdited}
                                    IsReadOnly={props.IsReadOnly}
                                />
                    break;
                default:
                    _content = null;
                    break
            }
            let columnSize = td.ColumnSize ? ' '+td.ColumnSize :''
            return <td key={key} className={`td${columnSize}`}>{_content}</td>
        })}</tr>
    })
    
    return(
        <table className="table table-default table-block">
            <tbody className="tbody">
                {contentRows}
            </tbody>
        </table>
    )
}

export class Table extends Component{
    componentDidMount=()=>{
        if(this.props.EnableOverlayScrollbar && process.env.NODE_ENV === 'production'){
            $(this.refs.table_container).overlayScrollbars({});
            $(this.refs.table_container__body).overlayScrollbars({});
        }
    }
    render(){
        let rows = this.props.Body.length === 0 ? <NoData DisplayText={this.props.NoDataDiplayText}/> : 
            this.props.Body && this.props.Body.map(function (data, idx) {
            let mainKey = `${this.props.TableName}-body-info-${idx}`;
            return <TableBodyRows key={mainKey} RowIdx={idx} Headers={this.props.Header} CombineField={this.props.CombineField} Datas={data} MainKey={mainKey} TableName={this.props.TableName}/>
        }, this)
        
        let tableSize = this.props.TableSize ? " "+this.props.TableSize : "";
        let tableType = this.props.TableType ? " "+this.props.TableType : "";
        let _tableClassName = this.props.TableClassName ? this.props.TableClassName : tableClassName.main__row_table;

        const headerTableOrder = this.props.HeaderTableType && this.props.HeaderTableType.TableOrder !== undefined ? this.props.HeaderTableType.TableOrder : tableOrder.primary;
        const bodyTableOrder = this.props.BodyTableType && this.props.BodyTableType.TableOrder !== undefined ? this.props.BodyTableType.TableOrder : tableOrder.primary;
        const _outerTableHeader = this.props.OuterTableHeader ? this.props.OuterTableHeader : null;
        return(
            <div className={_tableClassName}>
                {_outerTableHeader}
                <div className={`table-container${tableSize}`} ref="table_container">
                    <div className="table-container__head">
                        <table className={`table ${headerTableOrder}${tableType}`}>                                
                            <TableHeaders TableName={this.props.TableName} Datas={this.props.Header} ClickSorting={this.props.ClickSorting}/>
                        </table>
                    </div>
                    <div className="table-container__body" ref="table_container__body">
                        <table className={`table ${bodyTableOrder}${tableType}`}>
                            <tbody className="tbody">
                                {rows}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        )
    }
}

Table.defaultProps={
    EnableOverlayScrollbar: true
}

const NoData = (props)=>{
    return <tr className="tr"><td className='td'>{props.DisplayText}</td></tr>    
}

const TableHeaders = (props)=>{
    const HeaderRows = convertToHeaderRows(props.Datas, 0);
    let rows = HeaderRows.map((level, idx)=>{
        let _header = level.rows.map((data,idx)=>{
            return  <HeaderColumn key={`${props.TableName}-${data.DataColumn}-${idx}`} TableName={props.TableName} {...data} HandleSorting={props.ClickSorting ? props.ClickSorting.bind(null, props.TableName, data.DataColumn): null}/>
        })

        return (
            <tr className="tr" key={`${props.TableName}-level-${level}-${idx}`}>
                {_header}
            </tr>
        )    
    })

    return (
        <thead className="thead">
            {rows}
        </thead>
    )
}

const HeaderColumn = (props)=>{
    let _sortingBtn = props.HasSort ? <SortingBtn OrderBy={props.OrderBy} HandleSorting={props.HandleSorting}/> : null
    let _sortingClass = props.HasSort ? ' td--sort' : ''
    let _columnSize = props.ColumnSize ? " "+props.ColumnSize : ''
    let _align = props.Align ? " "+props.Align : ''
    
    let content = null;
    if(props.HeaderContent !== undefined){
        content = props.HeaderContent.map((info, idx)=>{
            let result = [];
            let key = `${props.TableName}-header-${props.DataColumn}-${idx}`
            switch(info.Type){
                case headerContent.Checkbox:
                    result = (
                        <CheckBox 
                            key={key}
                            Checked={info.Value}
                            Value={info.Value}
                            HandleClick={info.OnClick}
                        />  
                    );
                    break;
                default:
                    break;
            }

            return result;
        })
    }else{
        content = (
            <>
                <span className="text">{props.DisplayText}</span>
                {_sortingBtn}
            </>
        );
    }

    if(props.Rowspan !== undefined){
        return (
            <td className={`td${_columnSize}${_sortingClass}${_align}`} rowSpan={props.Rowspan}>
                {content}
            </td>
        )
    }
    else if(props.Colspan !== undefined){
        return(
            <td className={`td${_columnSize}${_sortingClass}${_align}`} colSpan={props.Colspan}>
                {content}
            </td>
        )
    }else{
        return(
            <td className={`td${_columnSize}${_sortingClass}${_align}`}>
                {content}
            </td>
        )
    }
}

const SortingBtn = (props)=>{
    return(
        <div className={`newbtn newbtn-sort ${props.OrderBy}`} onClick={props.HandleSorting}>
            <i className="icon icon-sort"></i>
        </div>
    )
}

const TableBodyRows = (props)=>{
    const AllHeaders = (Headers)=>{
        return getAllHeaders(Headers);
    }
    
    let datas = props.Datas;
    let rows = AllHeaders(props.Headers).map((colInfo,idx)=>{
        let key = `body-rows-${props.MainKey}-${colInfo.DataColumn}-${idx}`
        let matchTargetValue = false;
        if(colInfo.ReplaceValue){
            let condition = colInfo.ReplaceValue.Condition;
            matchTargetValue = condition && datas[condition.DataColumn].Value === condition.TargetValue
        }

        if(matchTargetValue){
            let value = colInfo.ReplaceValue.NewValueColumns.map(col=>{
                return datas[col].Value
            })
            return <TableBodyRow key={key} {...colInfo} MainKey={key} Value={value} RowIdx={props.RowIdx} UseReplaceValue={true}/>
        }else if(colInfo.SetColumnToEmpty && datas[colInfo.SetColumnToEmpty.When.DataColumn].Value === colInfo.SetColumnToEmpty.When.ValueIs){
            return <TableBodyRow key={key} {...colInfo} RowIdx={props.RowIdx} SetToEmpty={true}/>
        }else{
            let value = datas[colInfo.DataColumn].Value;
            
            if(colInfo.FieldContent){
                let parameters = colInfo.FieldContent.map(content=>{
                    let para = content.BindingField ? content.BindingField.filter(col=>{
                        return datas[col] !== undefined && datas[col] !== null
                    }).map(col=>{
                        return datas[col].Value
                    }) : props.RowIdx
                    return para
                })

                return <TableBodyRow key={key} {...colInfo} Value={value} Parameters={parameters} RowIdx={props.RowIdx} TableName={props.TableName}/>
            }else{
                return <TableBodyRow key={key} {...colInfo} Value={value} RowIdx={props.RowIdx}/>
            }
        }
    })

    return(
        <tr className="tr">
            {rows}
        </tr>
    )
}

const GetFormatValue = (Value, Format, RowIdx) =>{
    if(Format){
        switch(Format.FormatType){
            case fieldFormat.DateTime:
                let date = Format.ConvertToLocalTimeZone ? TimeZone.toLocalTimeZone(Value, -4) : Value                
                return new DateFormat(date).format(Format.FormatPattern)

            case fieldFormat.SequenceNumber:
                return RowIdx+1;

            case fieldFormat.DecimalRoundDown:
                let regex = /^[0-9]+\.?[0-9]*$/
                let _value = Value;
                let fixedPoint = Format.FormatPattern === null || Format.FormatPattern === undefined ? 0 : Format.FormatPattern;
                if(typeof(_value) === 'string' && _value.match(regex)){
                    let places = fixedPoint === 0 ? 100 : 10 ** Format.FormatPattern
                    _value = Math.round(_value * places) / places   
                }

                _value = _value.toFixed(fixedPoint)
                return _value;

            default:
                return Value
        }
    }else{
        return Value;
    }
}

const TableBodyRow = (props)=>{
    let _columnSize = props.ColumnSize ? " "+props.ColumnSize : ''
    let _align = props.BodyRowAlign === undefined ? props.Align ? " "+props.Align : '' : props.BodyRowAlign === '' ? props.BodyRowAlign : ' '+props.BodyRowAlign
    let _content = null;
    let _contentWithTd = false;
    
    if(props.Value !== undefined && props.Value !== null && typeof(props.Value) === 'object'){
        _content = <>{props.Value}</>
    }else{
        if(props.SetToEmpty){
            _content = <span></span>
        }else if(props.FieldContent){
            _content = props.FieldContent.map((content, idx)=>{
                let _fieldContent = filedContentTypeAndClass[content.Type];
                let _contentType = _fieldContent ? _fieldContent.Type : null;
                let key = `dataColumn_${props.DataColumn}-bodyRow_${props.RowIdx}-content_${idx}`
                switch(_contentType){
                    case fieldContentType.Icon:
                        if(content.DisplayCurrentValue){
                            let Icon = null;
                            let displayText = null;
                            if(content.WhenNull && !props.Value){
                                let _null_fieldContent = filedContentTypeAndClass[content.WhenNull.NullType];
                                Icon = <IconButton key={key} IconClass={_null_fieldContent.ClassName} OnClick={content.OnClick.bind(null, props.TableName, props.Parameters[idx])}/>
                                displayText = content.WhenNull.NullDisplayText
                            }else{
                                Icon = <IconButton IconClass={_fieldContent.ClassName} OnClick={content.OnClick.bind(null, props.TableName, props.Parameters[idx])}/>
                                displayText = props.Value
                            }

                            _contentWithTd = true;
                            return (
                                <td key={key} className={`td${_columnSize}${_align}`} >
                                    {Icon}
                                    <span>{displayText}</span>
                                </td>
                            )
                        }else{
                            if(content.WhenNull){
                                let _null_fieldContent = filedContentTypeAndClass[content.WhenNull.NullType];
                                return <IconButton key={key} IconClass={_null_fieldContent.ClassName} OnClick={content.OnClick.bind(null, props.TableName, props.Parameters[idx])}/>
                            }else{
                                return <IconButton key={key} IconClass={_fieldContent.ClassName} OnClick={content.OnClick.bind(null, props.TableName, props.Parameters[idx])}/>
                            }
                        }
                    case fieldContentType.Checkbox:
                        return <CheckBox 
                                    key={key}
                                    Checked={props.Value ? true : false}
                                    Value={props.Value ? props.Value : false}
                                    HandleClick={content.OnClick}
                                    BindingValue={props.Parameters[idx]}
                                    ConfirmAlert={content.ConfirmAlert}
                                />           
                    default:
                        return <span key={key}>{GetFormatValue(props.Value, props.FieldFormat, props.RowIdx)}</span>
                }
            })
        }else if(props.UseReplaceValue){
            _content = props.Value && props.Value.map((v,idx)=>{
                return <div key={`${props.MainKey}-ReplaceColumn-${idx}`}>{v}</div>
            });
        }else{
            _content = <span>{GetFormatValue(props.Value, props.FieldFormat, props.RowIdx)}</span>
        }
    }

    return _contentWithTd ? _content : (
        <td className={`td${_columnSize}${_align}`} >
            {_content}
        </td>
    );
}

const IconButton = (props)=>{
    return(
        <div className="newbtn newbtn-lighticon" onClick={props.OnClick}>
            <i className={`icon ${props.IconClass}`}></i>
        </div>
    )
}

const convertToHeaderRows = (headers, level)=>{
    let result = [];
    let rows = [];
    headers.filter(h=>{return h.HideColumn !== true}).forEach(h=>{
        rows.push(h);
        if(h.SubRows){            
            result.push({level:level+1, rows:convertToHeaderRows(h.SubRows, level+1)});
        }
    })

    result.push({level:level, rows:rows});
    if(level===0){
        return result.sort((a,b)=>{
            return a.level > b.level ? 1:-1;
        });
    }else{
        return rows;
    }    
}

const getAllHeaders = (headers, getHide=false)=>{
    let rows = [];
    headers.filter(d=>{return getHide ? true : d.HideColumn !== true}).forEach(d=>{        
        if(d.SubRows){
            rows = rows.concat(getAllHeaders(d.SubRows));
        }else{
            rows.push(d);
        }
    })
    return rows;
}

function getTableHeaderInfo(defineTableHeaders, sortingInfo){
    let columns = defineTableHeaders;
    if(sortingInfo !== null){
        columns = HeaderMappingOrderBy(columns, sortingInfo)
    }
    
    return columns
}
function HeaderMappingOrderBy(_headers, SortingInfo){
    return _headers.filter(col=>{
        return !col.IsForCombine
    }).map(col=>{
        if(col.SubRows){
            col.SubRows = HeaderMappingOrderBy(col.SubRows, SortingInfo)
        }else{
            if(SortingInfo !== null && SortingInfo[col.DataColumn] !== undefined){
                col['OrderBy'] = SortingInfo[col.DataColumn]
            }
        }
        
        return col
    })
}

export function convertToTableResult(tableName, defineTableHeaders, resultDatas, sortingInfo=null){
    let _headerColumns = getAllHeaders(defineTableHeaders, true);
    let _combineFields = [];
    let _body = resultDatas && resultDatas.map((data)=>{
        let result = {};
        let combineField = {};
        Object.keys(_headerColumns).forEach(column=>{
            let columnInfo = _headerColumns[column];
            if(columnInfo.IsForCombine){
                combineField[columnInfo.DataColumn] = data[columnInfo.DataColumn]
            }

            result[columnInfo.DataColumn] = {Value:data[columnInfo.DataColumn]}
        })
        
        if(combineField){
            _combineFields.push(combineField)
        }

        return result
    })

    return {
        'TableName':tableName,
        'Header':getTableHeaderInfo(defineTableHeaders, sortingInfo),
        'Body':_body,
        'CombineField':_combineFields
    }
}
