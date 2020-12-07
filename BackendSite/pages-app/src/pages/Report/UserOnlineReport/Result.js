import React from "react";

const dataColumnDefs = () => {
    return [
        { 
            columnTitle: 'Local Time(GMT +8)',
            dataColumn: 'localTime'
        },
        {
            columnTitle: 'Online',
            dataColumn: 'maxCount'
        },
    ]
}

const Panel = (props) => {
    console.log(props);
    const columnDefs = dataColumnDefs();
    let headerRows = [];

    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} 
                scope="col" 
                className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}
                style={{position:"sticky", top:"0"}}>
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
                    bgcolor={data.maxCount===props.MaxCount ? 'LemonChiffon' : ''}
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

                {/* <button type="button" className="btn btn-approved" onClick={props.ExportToExcel}>Export Excel</button> */}
            </div>
            <table className="table table-striped" style={{width:"auto"}}>
                <thead>
                    <tr>
                        {headerRows}
                    </tr>
                </thead>
                <tbody>
                    {bodyRows}
                </tbody>
            </table>
        </div>
    )
}

export default Panel;