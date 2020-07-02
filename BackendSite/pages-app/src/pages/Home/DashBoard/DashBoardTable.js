import React from "react";

export const DashBoardTable = (props) => {
    console.log(props.tableData);
    return (
        <td>
            <table className="table table-striped">
                <thead>
                    <tr>
                        <th scope="col">{props.tableData.title}</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>{props.tableData.data}</td>
                    </tr>
                </tbody>
            </table>
        </td>
    );
}