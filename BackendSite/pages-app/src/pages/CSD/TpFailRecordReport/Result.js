import React from "react";
import moment from 'moment';
import Pagination from './Pagination';

const dateFormat = (dateTime) => {
    return moment(dateTime).format('l LTS');
}

const alertInfo = (info) => {
    alert(info);
}

const dataColumnDefs = () => {
    return [
        {
            columnTitle: 'Site Name',
            dataColumn: "siteName",
            header: {
                style: { 
                    width:"5%",
                    position:"sticky",
                    top:"36.8px"
                }
            }
        },
        {
            columnTitle: 'User',
            dataColumn: "userName",
            header: {
                style: { 
                    width:"10%",
                    position:"sticky",
                    top:"36.8px"
                }
            }
        },
        { 
            columnTitle: 'Trans Time (GMT +8)',
            dataColumn: 'transTime',
            header: {
                style: { 
                    width:"15%",
                    position:"sticky",
                    top:"36.8px"
                }
            },
            body: { format: dateFormat }
        },
        { 
            columnTitle: 'Vendor',
            dataColumn: 'tpName',
            header: {
                style: { 
                    width:"5%",
                    position:"sticky",
                    top:"36.8px"
                }
            }
        },
        { 
            columnTitle: 'Trans Id',
            dataColumn: 'transId',
            header: {
                style: { 
                    width:"15%",
                    position:"sticky",
                    top:"36.8px"
                }
            }
        },
        { 
            columnTitle: 'Info',
            dataColumn: 'info',
            body:
            {
                style: { 
                    cursor: "pointer"
                },
                clickBack: alertInfo
            }
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
                style={element.header && element.header.style ? element.header.style : {position:"sticky", top:"36.8px"}}>
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
            if (element.dataColumn === "info")
            {
                return <td key={`td-${element.dataColumn}-${idx}`} 
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    style={element.body && element.body.style ? element.body.style : {}}
                    onClick={() => { alert(data[element.dataColumn]); }}>
                    {data[element.dataColumn]}
                </td>
            }
            return (
                <td key={`td-${element.dataColumn}-${idx}`} 
                    className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}
                    style={element.body && element.body.style ? element.body.style : {}}>
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