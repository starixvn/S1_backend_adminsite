import React from "react";
import moment from 'moment';
import Pagination from './Pagination';

const dateFormat = (createTime) => {
    return moment(createTime).add(8, "hour").format('l LTS');
}

const dataColumnDefs = () => {
    return [
        {
            columnTitle: 'Site Name',
            dataColumn: "siteName"
        },
        {
            columnTitle: 'User',
            dataColumn: "userName"
        },
        { 
            columnTitle: 'Trans Time (GMT +8)',
            dataColumn: 'transTime',
            body: { format: dateFormat }
        },
        { 
            columnTitle: 'Vendor',
            dataColumn: 'tpName'
        },
        { 
            columnTitle: 'Trans Id',
            dataColumn: 'transId'
        },
        { 
            columnTitle: 'Info',
            dataColumn: 'info'
        }
    ]
}

const Panel = (props) => {

    const columnDefs = dataColumnDefs();

    let headerRows = [];

    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col" 
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}
                style={{position:"sticky", top:"36.8px" }}>
                {element.columnTitle}
            </th>
        );
    });

    let bodyRows = props.Datas.map((data, idx) => {
        let row = columnDefs.map((element, idx) => {
            let subData = {};
            if (element.body && element.body.subData) {
                element.body.subData.forEach(s => { subData[s] = data[s] });
            }
            return (
                <td key={`td-${element.dataColumn}-${idx}`} 
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    >
                    {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                </td>
            );
        });

        return (
            <tr key={`result-body-${idx}`}>
                {row}
            </tr>
        )
    });

    return (
        <div className="table">
            <div className="table-options">
                <button type="button" className="btn btn-approved" onClick={props.ExportToExcel}>Export Excel</button>
            </div>
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
            <Pagination
                Data={{
                    totalPages: props.PageInfo.totalPages,
                    pageNumber: props.PageInfo.pageNumber,
                    rowCount: props.PageInfo.rowCount,
                    currencyAmount: props.PageInfo.currencyAmount
                }}
                ChangePage={props.ChangePage}
            />
        </div>
    )
}

export default Panel;