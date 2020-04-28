import React from "react";
import Pagination from '../../../component/Pagination';
import moment from 'moment';
import {number_format, number_toFixed} from '../../../module/Formatting/NumberFormat';
import {StatusList, adjustType} from './defColumns';

const dateFormat = (createTime)=>{
    return moment(createTime).format('l LTS');
}

const numberFormat = (number)=>{
    return number_format(number_toFixed(number, 2));
}

const statusFormat = (status)=>{
    return StatusList[status];
}

const adjustTypeFormat = (type)=>{
    return adjustType[type];
}

const dataColumnDefs = ()=>{
    return [
        {
            dataColumn:'siteName', columnTitle:'SiteName'
        },
        {
            dataColumn:'transId', columnTitle:'TransId'
        },
        {
            dataColumn:'adjustType', columnTitle:'Adjust Type', 
            body:{
                format:adjustTypeFormat
            }
        },
        {
            dataColumn:'amount', columnTitle:'Adjust Amount', 
            body:{
                format:numberFormat
            }
        },
        {
            dataColumn:'operator', columnTitle:'Operator'
        },
        {
            dataColumn:'createTime', 
            columnTitle:'CreateTime',
            body:{
                format:dateFormat
            }
        },
        {
            dataColumn:'status', columnTitle:'Status',
            body:{
                format:statusFormat
            }
        },
        {
            dataColumn:'memo', columnTitle:'Note'
        }
    ]
}

const Panel = (props)=>{
    const columnDefs = dataColumnDefs();
    let headerRows = [];
    
    const nodata = [];
    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} scope="col" className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}>
                {element.columnTitle}
            </th>
        );
        if(props.Datas.length === 0){
            nodata.push(<td key={`nodata-${idx}`}>{idx===0? "No Data." : null}</td>);
        }
    });

    let bodyRows = props.Datas.length === 0 ? <tr>{nodata}</tr> : props.Datas.map((data, idx)=>{
        let row = columnDefs.map((element, idx) => {
            let subData = {};
            if(element.body && element.body.subData){
                element.body.subData.forEach(s=>{ subData[s]=data[s]});
            }

            let displayValue = element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn];
            return ( 
                <td key={`td-${element.dataColumn}-${idx}`} className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''} title={displayValue}>
                    {displayValue}
                </td>
            );
        });

        return(
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
    });

    return(
        <div className="table">
            <table className="table table-striped">
                <thead>
                    <tr>
                        {headerRows}
                    </tr>
                </thead>
                <tbody>
                    {bodyRows}
                </tbody>
            </table>
            {
                props.Datas.length === 0 ? null : (
                    <Pagination 
                        Data={{
                            totalPages:props.PageInfo.totalPages,
                            pageNumber:props.PageInfo.pageNumber,
                            rowCount:props.PageInfo.rowCount
                        }}
                        ChangePage={props.ChangePage}
                    />
                )
            }
        </div>
    )
}

export default Panel;