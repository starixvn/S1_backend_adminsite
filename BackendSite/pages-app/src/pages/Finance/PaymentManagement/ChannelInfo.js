import React from "react";
import {CurrencyResultTable} from './CurrencyInfo';
import {ElemType, Mode, InputBoxFormat}  from './components';
import {ResultTable} from './ResultTable';

export const ChannelInfoHeader = (props)=>{
    return(
        <span style={{'color':'white'}}>{props.Name}</span>
    );
}

export const ChannelInfoContent = (props)=>{
    let {currencyList, ...channelInfo} = props;
    return(
        <div className="table">
            <ResultTable columnDefs={columnDefs()} {...channelInfo} />
            <div className="table-options"/>
            
            <CurrencyResultTable 
                CurrencyList={currencyList} 
                idxArr={props.idxArr} 
                ClickEdit={props.ClickEdit}
                ClickCancel={props.ClickCancel}
                ClickSave={props.ClickSave}
            />
        </div>
    )
}

const columnDefs = ()=>{
    return [
        {
            dataColumn:'type', columnTitle:'Type'
        },
        {
            dataColumn:'sort', columnTitle:'Sort',
            body:{
                EditMode:{
                    ElemType: ElemType.inputBox,
                    ElemClass: 'input-large',
                    Format:InputBoxFormat.number
                }
            }
        },
        {
            dataColumn:'status', columnTitle:'Status',
            body:{
                EditMode:{
                    ElemType: ElemType.switchBtn,
                    ElemClass: 'input-large'
                }
            }
        },
        {
            dataColumn:'mode', columnTitle:'Mode',
            body:{
                EditMode:{
                    ElemType: ElemType.selectBox,
                    ElemClass: 'custom-select',
                    Options:[...Object.keys(Mode).map(key=>{return {value:key, displayText:Mode[key]}})]
                }
            }
        },
        {
            columnTitle:'Action',
            header:{
                attr:{
                    className:'width-85'
                }
            },
            body:{
                SpecialItem:ElemType.actionBtn
            }
        }
    ]
}

