'use client'

import { Auction } from "@/types";
import { Table } from "flowbite-react";

type Props = {
    auction: Auction
}

export default function DetailedSpecs({auction}: Props) {
    return (
        <Table striped={true} className="divide-y divide-gray-300 rounded-lg shadow-md">
            <Table.Body className="divide-y divide-gray-200">
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white px-4 py-2">
                        Seller
                    </Table.Cell>
                    <Table.Cell className="px-4 py-2">
                        {auction.seller}
                    </Table.Cell>
                </Table.Row>
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white px-4 py-2">
                        Make
                    </Table.Cell>
                    <Table.Cell className="px-4 py-2">
                        {auction.make}
                    </Table.Cell>
                </Table.Row>
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white px-4 py-2">
                        Model
                    </Table.Cell>
                    <Table.Cell className="px-4 py-2">
                        {auction.model}
                    </Table.Cell>
                </Table.Row>
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white px-4 py-2">
                        Year manufactured
                    </Table.Cell>
                    <Table.Cell className="px-4 py-2">
                        {auction.year}
                    </Table.Cell>
                </Table.Row>
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white px-4 py-2">
                        Mileage
                    </Table.Cell>
                    <Table.Cell className="px-4 py-2">
                        {auction.mileage}
                    </Table.Cell>
                </Table.Row>
                <Table.Row className="bg-white dark:border-gray-700 dark:bg-gray-800">
                    <Table.Cell className="whitespace-nowrap font-medium text-gray-900 dark:text-white px-4 py-2">
                        Has reserve price?
                    </Table.Cell>
                    <Table.Cell className="px-4 py-2">
                        {auction.reservePrice > 0 ? 'Yes' : 'No'}
                    </Table.Cell>
                </Table.Row>
            </Table.Body>
        </Table>
    );
}
